namespace me_workspace.Web.Infrastructure.Speech;

public sealed class LocalSpeechToTextClient(OfflineSpeechEngineRunner speechEngineRunner) : ISpeechToTextClient
{
    public Task<string> TranscribeAsync(Stream audioStream, CancellationToken cancellationToken)
    {
        var message = speechEngineRunner.IsConfigured
            ? "Local speech engine is configured. Dictation can now hand captured audio to the offline engine when you stop recording."
            : "Voice draft from the local speech adapter. Configure a local offline engine to replace this placeholder path.";

        return Task.FromResult(message);
    }
}
