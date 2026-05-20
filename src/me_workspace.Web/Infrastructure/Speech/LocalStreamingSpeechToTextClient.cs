using System.Collections.Concurrent;

namespace me_workspace.Web.Infrastructure.Speech;

public sealed class LocalStreamingSpeechToTextClient : IStreamingSpeechToTextClient
{
    private readonly ConcurrentDictionary<Guid, LocalSpeechSession> sessions = new();

    public Task<SpeechSessionState> StartSessionAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var session = new LocalSpeechSession
        {
            SessionId = Guid.NewGuid(),
            Status = "recording",
            Transcript = "Local dictation session started. Connect a real offline speech worker to stream transcript text here.",
            Final = false,
            ChunkCount = 0,
            StartedUtc = now,
            UpdatedUtc = now,
        };

        sessions[session.SessionId] = session;
        return Task.FromResult(session.ToState());
    }

    public Task<SpeechSessionState?> GetSessionAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        return Task.FromResult(sessions.TryGetValue(sessionId, out var session) ? session.ToState() : null);
    }

    public async Task<SpeechSessionState?> AppendAudioChunkAsync(Guid sessionId, Stream audioStream, CancellationToken cancellationToken)
    {
        if (!sessions.TryGetValue(sessionId, out var session))
        {
            return null;
        }

        await using var buffer = new MemoryStream();
        await audioStream.CopyToAsync(buffer, cancellationToken);

        session.ChunkCount += 1;
        session.UpdatedUtc = DateTimeOffset.UtcNow;
        session.Final = false;
        session.Transcript = $"Local dictation session received {session.ChunkCount} audio chunk(s). Replace the placeholder local speech worker to produce live transcript text.";

        return session.ToState();
    }

    public Task<SpeechSessionState?> StopSessionAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        if (!sessions.TryGetValue(sessionId, out var session))
        {
            return Task.FromResult<SpeechSessionState?>(null);
        }

        session.Status = "stopped";
        session.Final = true;
        session.UpdatedUtc = DateTimeOffset.UtcNow;
        return Task.FromResult<SpeechSessionState?>(session.ToState());
    }

    private sealed class LocalSpeechSession
    {
        public required Guid SessionId { get; init; }
        public required string Status { get; set; }
        public required string Transcript { get; set; }
        public required bool Final { get; set; }
        public required int ChunkCount { get; set; }
        public required DateTimeOffset StartedUtc { get; init; }
        public required DateTimeOffset UpdatedUtc { get; set; }

        public SpeechSessionState ToState() =>
            new(SessionId, Status, Transcript, Final, ChunkCount, StartedUtc, UpdatedUtc);
    }
}
