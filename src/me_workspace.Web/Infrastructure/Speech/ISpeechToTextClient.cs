namespace me_workspace.Web.Infrastructure.Speech;

public interface ISpeechToTextClient
{
    Task<string> TranscribeAsync(Stream audioStream, CancellationToken cancellationToken);
}
