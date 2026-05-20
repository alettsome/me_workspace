namespace me_workspace.Web.Infrastructure.Speech;

public interface IStreamingSpeechToTextClient
{
    Task<SpeechSessionState> StartSessionAsync(CancellationToken cancellationToken);
    Task<SpeechSessionState?> GetSessionAsync(Guid sessionId, CancellationToken cancellationToken);
    Task<SpeechSessionState?> AppendAudioChunkAsync(Guid sessionId, Stream audioStream, CancellationToken cancellationToken);
    Task<SpeechSessionState?> StopSessionAsync(Guid sessionId, CancellationToken cancellationToken);
}

public sealed record SpeechSessionState(
    Guid SessionId,
    string Status,
    string Transcript,
    bool Final,
    int ChunkCount,
    DateTimeOffset StartedUtc,
    DateTimeOffset UpdatedUtc);
