using Microsoft.Extensions.Options;
using me_workspace.Web.Infrastructure.Workspace;
using System.Text;
using System.Text.RegularExpressions;

namespace me_workspace.Web.Features.Chunking;

public sealed class ChunkingService(IOptions<WorkspaceOptions> options, ILogger<ChunkingService> logger)
{
    private readonly WorkspaceOptions _options = options.Value;

    public int TargetChunkTokens => _options.TargetChunkTokens;
    public int ChunkOverlapTokens => _options.ChunkOverlapTokens;

    /// <summary>
    /// Chunk text by conversation turns (User/AI exchanges)
    /// Suitable for ChatGPT conversations and dialogue-based content
    /// </summary>
    public List<ConversationChunk> ChunkByConversationTurns(string text, string sourceTitle)
    {
        var chunks = new List<ConversationChunk>();
        var turns = ParseConversationTurns(text);
        
        if (!turns.Any())
        {
            logger.LogWarning("No conversation turns found in {SourceTitle}. Using simple chunking.", sourceTitle);
            return ChunkBySize(text, sourceTitle);
        }

        logger.LogInformation("Found {TurnCount} conversation turns in {SourceTitle}", turns.Count, sourceTitle);

        var chunkIndex = 0;
        var currentChunk = new StringBuilder();
        var currentTurnCount = 0;
        var startTurn = 0;

        for (int i = 0; i < turns.Count; i++)
        {
            var turn = turns[i];
            var turnText = $"\n[{turn.Speaker}]\n{turn.Content}\n";
            var estimatedTokens = EstimateTokenCount(currentChunk.ToString() + turnText);

            // If adding this turn would exceed target, save current chunk
            if (currentTurnCount > 0 && estimatedTokens > TargetChunkTokens)
            {
                chunks.Add(new ConversationChunk
                {
                    Index = chunkIndex++,
                    Content = currentChunk.ToString().Trim(),
                    StartTurn = startTurn,
                    EndTurn = i - 1,
                    TurnCount = currentTurnCount,
                    EstimatedTokens = EstimateTokenCount(currentChunk.ToString())
                });

                // Start new chunk with overlap (include last turn for context)
                currentChunk.Clear();
                currentChunk.AppendLine($"[Previous conversation...]");
                currentTurnCount = 0;
                startTurn = i;
            }

            currentChunk.Append(turnText);
            currentTurnCount++;
        }

        // Add final chunk
        if (currentTurnCount > 0)
        {
            chunks.Add(new ConversationChunk
            {
                Index = chunkIndex,
                Content = currentChunk.ToString().Trim(),
                StartTurn = startTurn,
                EndTurn = turns.Count - 1,
                TurnCount = currentTurnCount,
                EstimatedTokens = EstimateTokenCount(currentChunk.ToString())
            });
        }

        logger.LogInformation("Created {ChunkCount} conversation chunks from {TurnCount} turns", chunks.Count, turns.Count);
        return chunks;
    }

    /// <summary>
    /// Simple chunking by character count for non-conversational text
    /// </summary>
    public List<ConversationChunk> ChunkBySize(string text, string sourceTitle)
    {
        var chunks = new List<ConversationChunk>();
        var targetChars = TargetChunkTokens * 4; // Rough estimate: 1 token ≈ 4 chars
        var lines = text.Split('\n');
        
        var chunkIndex = 0;
        var currentChunk = new StringBuilder();
        var currentLineStart = 0;
        var currentLineCount = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var estimatedChars = currentChunk.Length + line.Length + 1;

            if (currentLineCount > 0 && estimatedChars > targetChars)
            {
                chunks.Add(new ConversationChunk
                {
                    Index = chunkIndex++,
                    Content = currentChunk.ToString().Trim(),
                    StartTurn = currentLineStart,
                    EndTurn = i - 1,
                    TurnCount = currentLineCount,
                    EstimatedTokens = EstimateTokenCount(currentChunk.ToString())
                });

                currentChunk.Clear();
                currentLineStart = i;
                currentLineCount = 0;
            }

            currentChunk.AppendLine(line);
            currentLineCount++;
        }

        if (currentLineCount > 0)
        {
            chunks.Add(new ConversationChunk
            {
                Index = chunkIndex,
                Content = currentChunk.ToString().Trim(),
                StartTurn = currentLineStart,
                EndTurn = lines.Length - 1,
                TurnCount = currentLineCount,
                EstimatedTokens = EstimateTokenCount(currentChunk.ToString())
            });
        }

        return chunks;
    }

    private List<ConversationTurn> ParseConversationTurns(string text)
    {
        var turns = new List<ConversationTurn>();
        
        // Pattern for ChatGPT conversations: Look for common speaker indicators
        // Examples: "User:", "You:", "Assistant:", "ChatGPT:", or page markers followed by content
        var lines = text.Split('\n');
        ConversationTurn? currentTurn = null;
        var currentContent = new StringBuilder();

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            
            // Skip empty lines and page markers
            if (string.IsNullOrWhiteSpace(trimmedLine) || 
                trimmedLine.StartsWith("===") || 
                trimmedLine.StartsWith("---"))
            {
                continue;
            }

            // Check if this line starts a new turn
            var speaker = DetectSpeaker(trimmedLine);
            
            if (speaker != null)
            {
                // Save previous turn if exists
                if (currentTurn != null && currentContent.Length > 0)
                {
                    currentTurn.Content = currentContent.ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(currentTurn.Content))
                    {
                        turns.Add(currentTurn);
                    }
                }

                // Start new turn
                currentTurn = new ConversationTurn { Speaker = speaker };
                currentContent.Clear();
                
                // Remove speaker prefix from line
                var content = RemoveSpeakerPrefix(trimmedLine);
                if (!string.IsNullOrWhiteSpace(content))
                {
                    currentContent.AppendLine(content);
                }
            }
            else if (currentTurn != null)
            {
                // Continue current turn
                currentContent.AppendLine(trimmedLine);
            }
            else
            {
                // No turn started yet, treat as system/intro text
                if (turns.Count == 0)
                {
                    currentTurn = new ConversationTurn { Speaker = "System" };
                    currentContent.AppendLine(trimmedLine);
                }
            }
        }

        // Save final turn
        if (currentTurn != null && currentContent.Length > 0)
        {
            currentTurn.Content = currentContent.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(currentTurn.Content))
            {
                turns.Add(currentTurn);
            }
        }

        return turns;
    }

    private string? DetectSpeaker(string line)
    {
        // Common patterns in ChatGPT exports
        if (Regex.IsMatch(line, @"^(You|User)[\s:]", RegexOptions.IgnoreCase))
            return "User";
        
        if (Regex.IsMatch(line, @"^(ChatGPT|Assistant|AI|GPT)[\s:]", RegexOptions.IgnoreCase))
            return "Assistant";
        
        // Check for common question/statement patterns that indicate user input
        if (line.StartsWith("I want", StringComparison.OrdinalIgnoreCase) ||
            line.StartsWith("I need", StringComparison.OrdinalIgnoreCase) ||
            line.StartsWith("Can you", StringComparison.OrdinalIgnoreCase) ||
            line.StartsWith("How do", StringComparison.OrdinalIgnoreCase))
        {
            return "User";
        }

        return null;
    }

    private string RemoveSpeakerPrefix(string line)
    {
        // Remove common speaker prefixes
        var cleaned = Regex.Replace(line, @"^(You|User|ChatGPT|Assistant|AI|GPT)[\s:]+", "", RegexOptions.IgnoreCase);
        return cleaned;
    }

    private int EstimateTokenCount(string text)
    {
        // Rough estimate: ~4 characters per token (conservative for English)
        // More accurate would use tiktoken, but this is good enough for chunking
        return text.Length / 4;
    }
}

public class ConversationTurn
{
    public required string Speaker { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class ConversationChunk
{
    public int Index { get; set; }
    public required string Content { get; set; }
    public int StartTurn { get; set; }
    public int EndTurn { get; set; }
    public int TurnCount { get; set; }
    public int EstimatedTokens { get; set; }
}
