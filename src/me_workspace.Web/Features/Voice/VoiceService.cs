using me_workspace.Web.Infrastructure.Speech;

namespace me_workspace.Web.Features.Voice;

public sealed class VoiceService(
    ISpeechToTextClient speechToTextClient,
    IStreamingSpeechToTextClient streamingSpeechToTextClient)
{
    public async Task<VoiceTranscriptDto> CreateDraftTranscriptAsync(CancellationToken cancellationToken)
    {
        await using var audioStream = new MemoryStream();
        var transcript = await speechToTextClient.TranscribeAsync(audioStream, cancellationToken);

        if (string.IsNullOrWhiteSpace(transcript))
        {
            transcript = "Draft voice transcript ready. You can edit this and send it through the local chat flow.";
        }

        return new VoiceTranscriptDto(transcript, true);
    }

    public async Task<VoiceSessionDto> StartSessionAsync(CancellationToken cancellationToken)
    {
        var session = await streamingSpeechToTextClient.StartSessionAsync(cancellationToken);
        return MapSession(session);
    }

    public async Task<VoiceSessionDto?> GetSessionAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        var session = await streamingSpeechToTextClient.GetSessionAsync(sessionId, cancellationToken);
        return session is null ? null : MapSession(session);
    }

    public async Task<VoiceSessionDto?> AppendAudioChunkAsync(Guid sessionId, Stream audioStream, CancellationToken cancellationToken)
    {
        var session = await streamingSpeechToTextClient.AppendAudioChunkAsync(sessionId, audioStream, cancellationToken);
        return session is null ? null : MapSession(session);
    }

    public async Task<VoiceSessionDto?> StopSessionAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        var session = await streamingSpeechToTextClient.StopSessionAsync(sessionId, cancellationToken);
        return session is null ? null : MapSession(session);
    }

    public string GetStatusSummary() =>
        "Voice currently supports a local draft path, and the backend now exposes a local streaming session contract for offline dictation.";

    private static VoiceSessionDto MapSession(SpeechSessionState session) =>
        new(
            session.SessionId,
            session.Status,
            session.Transcript,
            session.Final,
            session.ChunkCount,
            session.StartedUtc,
            session.UpdatedUtc);
}
