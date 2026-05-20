namespace me_workspace.Web.Infrastructure.Speech;

public sealed class SpeechEngineOptions
{
    public const string SectionName = "Speech";

    public string Provider { get; set; } = "Placeholder";
    public string ExecutablePath { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
    public string WorkingDirectory { get; set; } = string.Empty;
    public string InputFileExtension { get; set; } = ".webm";
    public string ModelPath { get; set; } = string.Empty;
    public string Language { get; set; } = "en";
    public bool TranslateToEnglish { get; set; }
    public bool NoTimestamps { get; set; } = true;
    public int Threads { get; set; }
}
