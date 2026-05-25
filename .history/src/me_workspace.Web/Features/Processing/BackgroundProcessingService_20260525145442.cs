using me_workspace.Web.Features.Sources;
using me_workspace.Web.Infrastructure.Workspace;
using System.Diagnostics;

namespace me_workspace.Web.Features.Processing;

/// <summary>
/// Background service that watches the inbox folder and automatically processes new files
/// </summary>
public sealed class BackgroundProcessingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly WorkspaceLayoutService _workspaceLayout;
    private readonly ILogger<BackgroundProcessingService> _logger;
    private FileSystemWatcher? _watcher;
    private readonly HashSet<string> _processingFiles = new();
    private readonly object _lock = new();

    public BackgroundProcessingService(
        IServiceProvider serviceProvider,
        WorkspaceLayoutService workspaceLayout,
        ILogger<BackgroundProcessingService> logger)
    {
        _serviceProvider = serviceProvider;
        _workspaceLayout = workspaceLayout;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var inboxPath = Path.Combine(_workspaceLayout.RuntimeRoot, "01-Inbox");
        
        if (!Directory.Exists(inboxPath))
        {
            _logger.LogWarning("Inbox folder not found at {InboxPath}. Creating it.", inboxPath);
            Directory.CreateDirectory(inboxPath);
        }

        _logger.LogInformation("Starting background file watcher on: {InboxPath}", inboxPath);

        _watcher = new FileSystemWatcher(inboxPath)
        {
            Filter = "*.pdf",
            EnableRaisingEvents = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
        };

        _watcher.Created += OnFileCreated;
        _watcher.Changed += OnFileChanged;

        _logger.LogInformation("Background file watcher started. Monitoring for new PDFs...");

        return Task.CompletedTask;
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation("New file detected: {FileName}", e.Name);
        _ = ProcessFileAsync(e.FullPath);
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        // File might still be copying, ignore for now
        // We'll handle it on Created event
    }

    private async Task ProcessFileAsync(string filePath)
    {
        // Prevent duplicate processing
        lock (_lock)
        {
            if (_processingFiles.Contains(filePath))
            {
                _logger.LogDebug("File {FilePath} already being processed, skipping", filePath);
                return;
            }
            _processingFiles.Add(filePath);
        }

        try
        {
            var fileName = Path.GetFileName(filePath);
            _logger.LogInformation("🔄 Starting automatic processing: {FileName}", fileName);

            // Wait for file to be fully written (sometimes file system events fire before copy completes)
            await WaitForFileReadyAsync(filePath);

            // Step 1: Extract PDF to text
            _logger.LogInformation("📄 Step 1: Extracting text from PDF...");
            var textPath = await ExtractPdfTextAsync(filePath);
            
            if (string.IsNullOrEmpty(textPath))
            {
                _logger.LogError("❌ PDF extraction failed for {FileName}", fileName);
                return;
            }

            _logger.LogInformation("✓ Text extracted: {TextPath}", Path.GetFileName(textPath));

            // Step 2: Register source in database
            _logger.LogInformation("📝 Step 2: Registering source in database...");
            var sourceId = await RegisterSourceAsync(filePath, textPath);
            
            if (sourceId == Guid.Empty)
            {
                _logger.LogError("❌ Source registration failed for {FileName}", fileName);
                return;
            }

            _logger.LogInformation("✓ Source registered: {SourceId}", sourceId);

            // Step 3: Chunk the content
            _logger.LogInformation("✂️ Step 3: Chunking content...");
            var result = await ChunkSourceAsync(sourceId);
            
            if (result.Success)
            {
                _logger.LogInformation("✅ Processing complete for {FileName}!", fileName);
                _logger.LogInformation("   📊 Stats: {ChunkCount} chunks, {CharCount} characters, {TimeMs}ms", 
                    result.ChunksCreated, result.CharactersProcessed, result.ProcessingTimeMs);
                
                // TODO: Send notification to UI (SignalR, WebSocket, or notification endpoint)
                await SendNotificationAsync(fileName, result);
            }
            else
            {
                _logger.LogError("❌ Chunking failed for {FileName}: {Message}", fileName, result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Automatic processing failed for {FilePath}", filePath);
        }
        finally
        {
            lock (_lock)
            {
                _processingFiles.Remove(filePath);
            }
        }
    }

    private async Task WaitForFileReadyAsync(string filePath, int maxAttempts = 10)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            try
            {
                // Try to open file exclusively - if it succeeds, file is ready
                using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                return; // File is ready
            }
            catch (IOException)
            {
                // File still being written, wait
                await Task.Delay(500);
            }
        }

        _logger.LogWarning("File {FilePath} may not be fully written after {Attempts} attempts", filePath, maxAttempts);
    }

    private async Task<string?> ExtractPdfTextAsync(string pdfPath)
    {
        try
        {
            var extractorPath = Path.Combine(_workspaceLayout.WorkspaceRoot, "tools", "ExtractPdfText", "bin", "Debug", "net8.0", "ExtractPdfText.exe");
            
            if (!File.Exists(extractorPath))
            {
                _logger.LogError("ExtractPdfText.exe not found at {ExtractorPath}", extractorPath);
                return null;
            }

            var textPath = Path.ChangeExtension(pdfPath, ".txt");

            var startInfo = new ProcessStartInfo
            {
                FileName = extractorPath,
                Arguments = $"\"{pdfPath}\" \"{textPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                _logger.LogError("Failed to start ExtractPdfText process");
                return null;
            }

            await process.WaitForExitAsync();

            if (process.ExitCode == 0 && File.Exists(textPath))
            {
                return textPath;
            }

            var error = await process.StandardError.ReadToEndAsync();
            _logger.LogError("ExtractPdfText failed with exit code {ExitCode}: {Error}", process.ExitCode, error);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting PDF text");
            return null;
        }
    }

    private async Task<Guid> RegisterSourceAsync(string pdfPath, string textPath)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var sourceRegistry = scope.ServiceProvider.GetRequiredService<SourceRegistryService>();

            // Scan will pick up both PDF and TXT
            var result = await sourceRegistry.ScanInboxAsync(CancellationToken.None);
            
            // Find the source we just added
            var fileName = Path.GetFileName(pdfPath);
            var source = result.RegisteredSources.FirstOrDefault(s => s.Title.Contains(Path.GetFileNameWithoutExtension(fileName)));
            
            return source?.Id ?? Guid.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering source");
            return Guid.Empty;
        }
    }

    private async Task<ProcessingResultDto> ChunkSourceAsync(Guid sourceId)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var pipelineService = scope.ServiceProvider.GetRequiredService<ProcessingPipelineService>();

            return await pipelineService.ProcessSourceAsync(sourceId, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error chunking source");
            return ProcessingResultDto.CreateFailure($"Chunking failed: {ex.Message}");
        }
    }

    private Task SendNotificationAsync(string fileName, ProcessingResultDto result)
    {
        // TODO: Implement notification mechanism
        // Options:
        // 1. SignalR hub to push to connected clients
        // 2. Store in database table for UI to poll
        // 3. Write to notification log file
        // 4. Email/webhook for external notifications

        _logger.LogInformation("📢 NOTIFICATION: {FileName} processed successfully - {ChunkCount} chunks created", 
            fileName, result.ChunksCreated);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        if (_watcher != null)
        {
            _watcher.Created -= OnFileCreated;
            _watcher.Changed -= OnFileChanged;
            _watcher.Dispose();
            _watcher = null;
        }

        base.Dispose();
    }
}
