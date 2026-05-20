namespace me_workspace.Web.Infrastructure.Speech;

public sealed class LocalSpeechToTextClient : ISpeechToTextClient
{
    public Task<string> TranscribeAsync(Stream audioStream, CancellationToken cancellationToken)
    {
        return Task.FromResult("Voice draft from the local speech adapter. Edit it if needed, then send it.");
    }
}
