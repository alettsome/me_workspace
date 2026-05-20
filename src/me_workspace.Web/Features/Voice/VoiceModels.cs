namespace me_workspace.Web.Features.Voice;

public sealed record VoiceTranscriptDto(string Text, bool Final);

public sealed record VoiceSessionDto(
    Guid SessionId,
    string Status,
    string Transcript,
    bool Final,
    int ChunkCount,
    DateTimeOffset StartedUtc,
    DateTimeOffset UpdatedUtc);
