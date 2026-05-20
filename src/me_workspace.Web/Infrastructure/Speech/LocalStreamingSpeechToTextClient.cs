using System.Collections.Concurrent;

namespace me_workspace.Web.Infrastructure.Speech;

public sealed class LocalStreamingSpeechToTextClient(OfflineSpeechEngineRunner speechEngineRunner) : IStreamingSpeechToTextClient
{
    private readonly ConcurrentDictionary<Guid, LocalSpeechSession> sessions = new();

    public Task<SpeechSessionState> StartSessionAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var session = new LocalSpeechSession
        {
            SessionId = Guid.NewGuid(),
            Status = "recording",
            Transcript = string.Empty,
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
        session.AudioBuffer.SetLength(0);
        session.AudioBuffer.Position = 0;
        buffer.Position = 0;
        await buffer.CopyToAsync(session.AudioBuffer, cancellationToken);
        session.AudioBuffer.Position = 0;
        session.UpdatedUtc = DateTimeOffset.UtcNow;
        session.Status = "recording";
        session.Final = false;

        if (speechEngineRunner.IsConfigured && session.AudioBuffer.Length > 44)
        {
            var transcript = await speechEngineRunner.TranscribeAsync(session.AudioBuffer.ToArray(), cancellationToken);
            if (!string.IsNullOrWhiteSpace(transcript))
            {
                session.Transcript = transcript;
            }
        }

        return session.ToState();
    }

    public async Task<SpeechSessionState?> StopSessionAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        if (!sessions.TryGetValue(sessionId, out var session))
        {
            return null;
        }

        session.Status = "stopped";
        session.UpdatedUtc = DateTimeOffset.UtcNow;

        if (speechEngineRunner.IsConfigured && session.AudioBuffer.Length > 0)
        {
            var transcript = await speechEngineRunner.TranscribeAsync(session.AudioBuffer.ToArray(), cancellationToken);
            if (!string.IsNullOrWhiteSpace(transcript))
            {
                session.Transcript = transcript;
            }
        }
        else if (session.ChunkCount == 0)
        {
            session.Transcript = "No audio was captured for this dictation session.";
        }

        session.Final = true;
        return session.ToState();
    }

    private sealed class LocalSpeechSession
    {
        public required Guid SessionId { get; init; }
        public MemoryStream AudioBuffer { get; } = new();
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
