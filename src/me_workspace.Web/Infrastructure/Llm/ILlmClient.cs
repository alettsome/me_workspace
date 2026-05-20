namespace me_workspace.Web.Infrastructure.Llm;

public interface ILlmClient
{
    Task<string> GenerateReplyAsync(string prompt, CancellationToken cancellationToken);
}
