using me_workspace.Web.Infrastructure.Speech;

namespace me_workspace.Web.Features.Voice;

public sealed class VoiceService(ISpeechToTextClient speechToTextClient)
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

    public string GetStatusSummary() =>
        "Voice is wired as a local draft transcript step so the frontend, speech adapter, and chat composer can connect cleanly.";
}
