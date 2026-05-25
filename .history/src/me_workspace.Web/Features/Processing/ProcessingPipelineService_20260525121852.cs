using Microsoft.EntityFrameworkCore;
using me_workspace.Web.Data;
using me_workspace.Web.Data.Entities;
using me_workspace.Web.Features.Chunking;
using me_workspace.Web.Infrastructure.Workspace;

namespace me_workspace.Web.Features.Processing;

public sealed class ProcessingPipelineService(
    AppDbContext db,
    WorkspaceLayoutService workspaceLayout,
    ChunkingService chunkingService,
    ILogger<ProcessingPipelineService> logger)
{
    /// <summary>
    /// Process a source through the full pipeline: Extract → Chunk → (Summarize)
    /// </summary>
    public async Task<ProcessingResultDto> ProcessSourceAsync(Guid sourceId, CancellationToken cancellationToken)
    {
        var source = await db.Sources
            .Include(s => s.Files)
            .FirstOrDefaultAsync(s => s.Id == sourceId, cancellationToken);

        if (source is null)
        {
            return ProcessingResultDto.Failed($"Source {sourceId} not found");
        }

        logger.LogInformation("Starting processing pipeline for source {SourceKey}", source.SourceKey);

        try
        {
            // Step 1: Load extracted text
            var textContent = await LoadTextContentAsync(source);
            if (string.IsNullOrWhiteSpace(textContent))
            {
                return ProcessingResultDto.Failed("No text content found or extracted");
            }

            logger.LogInformation("Loaded {CharCount} characters from {SourceKey}", textContent.Length, source.SourceKey);

            // Step 2: Chunk the content
            var chunks = await ChunkContentAsync(source, textContent);
            logger.LogInformation("Created {ChunkCount} chunks for {SourceKey}", chunks.Count, source.SourceKey);

            // Step 3: Save chunks to database
            await SaveChunksAsync(source, chunks, cancellationToken);
            
            // Step 4: Update source status
            source.CurrentStage = "Chunked";
            source.Status = "ChunksReady";
            source.UpdatedUtc = DateTime.UtcNow;
            
            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Processing pipeline completed for {SourceKey}: {ChunkCount} chunks created", 
                source.SourceKey, chunks.Count);

            return ProcessingResultDto.Success(
                $"Processed {source.Title}",
                chunks.Count,
                textContent.Length);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Processing pipeline failed for source {SourceKey}", source.SourceKey);
            
            // Update source with error
            source.Status = "Failed";
            source.UpdatedUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);

            return ProcessingResultDto.Failed($"Processing failed: {ex.Message}");
        }
    }

    private async Task<string> LoadTextContentAsync(Source source)
    {
        // Check if we already have extracted text (from PDF extraction tool)
        var originalFile = source.Files.FirstOrDefault(f => f.FileRole == "original");
        if (originalFile == null)
        {
            throw new InvalidOperationException("Source has no original file");
        }

        var absolutePath = Path.Combine(workspaceLayout.RuntimeRoot, originalFile.RelativePath);
        var extension = Path.GetExtension(absolutePath).ToLowerInvariant();

        // If PDF, look for extracted .txt file
        if (extension == ".pdf")
        {
            var txtPath = Path.ChangeExtension(absolutePath, ".txt");
            if (File.Exists(txtPath))
            {
                logger.LogInformation("Found extracted text file: {TxtPath}", txtPath);
                return await File.ReadAllTextAsync(txtPath);
            }
            else
            {
                logger.LogWarning("PDF text extraction not found at {TxtPath}. Run ExtractPdfText tool first.", txtPath);
                throw new InvalidOperationException("PDF text extraction required. Run ExtractPdfText tool first.");
            }
        }

        // For text files, read directly
        if (extension == ".txt" || extension == ".md")
        {
            return await File.ReadAllTextAsync(absolutePath);
        }

        throw new NotSupportedException($"File type {extension} not supported for text extraction");
    }

    private Task<List<ConversationChunk>> ChunkContentAsync(Source source, string textContent)
    {
        // Determine chunking strategy based on source type
        var isConversation = source.SourceType.Equals("conversation-log", StringComparison.OrdinalIgnoreCase) ||
                           source.SourceType.Equals("ChatExport", StringComparison.OrdinalIgnoreCase);

        List<ConversationChunk> chunks;
        
        if (isConversation)
        {
            logger.LogInformation("Using conversation-based chunking for {SourceKey}", source.SourceKey);
            chunks = chunkingService.ChunkByConversationTurns(textContent, source.Title);
        }
        else
        {
            logger.LogInformation("Using size-based chunking for {SourceKey}", source.SourceKey);
            chunks = chunkingService.ChunkBySize(textContent, source.Title);
        }

        return Task.FromResult(chunks);
    }

    private async Task SaveChunksAsync(Source source, List<ConversationChunk> chunks, CancellationToken cancellationToken)
    {
        // Remove existing chunks if re-processing
        var existingChunks = await db.Chunks
            .Where(c => c.SourceId == source.Id)
            .ToListAsync(cancellationToken);
        
        if (existingChunks.Any())
        {
            logger.LogInformation("Removing {Count} existing chunks for re-processing", existingChunks.Count);
            db.Chunks.RemoveRange(existingChunks);
        }

        // Create chunks folder for this source
        var chunksFolder = Path.Combine(
            workspaceLayout.ChunkedRoot, 
            source.SourceKey);
        
        Directory.CreateDirectory(chunksFolder);

        // Save each chunk
        var now = DateTime.UtcNow;
        foreach (var chunk in chunks)
        {
            // Save chunk text to file
            var chunkFileName = $"chunk-{chunk.Index:D4}.txt";
            var chunkFilePath = Path.Combine(chunksFolder, chunkFileName);
            await File.WriteAllTextAsync(chunkFilePath, chunk.Content, cancellationToken);

            // Get relative path for database
            var relativePath = Path.GetRelativePath(workspaceLayout.RuntimeRoot, chunkFilePath)
                .Replace('\\', '/');

            // Create database entry
            var dbChunk = new Chunk
            {
                SourceId = source.Id,
                ChunkIndex = chunk.Index,
                SectionTitle = $"Turns {chunk.StartTurn}-{chunk.EndTurn}",
                PageReference = null,
                TextPath = relativePath,
                CharacterCount = chunk.Content.Length,
                TokenCount = chunk.EstimatedTokens,
                Status = "Ready",
                CreatedUtc = now
            };

            db.Chunks.Add(dbChunk);
        }

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Saved {ChunkCount} chunks to database and {FolderPath}", 
            chunks.Count, chunksFolder);
    }
}

public record ProcessingResultDto(
    bool Success,
    string Message,
    int ChunksCreated,
    int CharactersProcessed)
{
    public static ProcessingResultDto Success(string message, int chunks, int chars) =>
        new(true, message, chunks, chars);

    public static ProcessingResultDto Failed(string message) =>
        new(false, message, 0, 0);
}
