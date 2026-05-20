using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace me_workspace.Web.Infrastructure.Speech;

public sealed class OfflineSpeechEngineRunner(
    IOptions<SpeechEngineOptions> options,
    IHostEnvironment hostEnvironment)
{
    private readonly SpeechEngineOptions speechOptions = options.Value;
    private readonly string contentRootPath = hostEnvironment.ContentRootPath;

    public bool IsConfigured =>
        (string.Equals(speechOptions.Provider, "ExternalProcess", StringComparison.OrdinalIgnoreCase) ||
         string.Equals(speechOptions.Provider, "WhisperCpp", StringComparison.OrdinalIgnoreCase)) &&
        !string.IsNullOrWhiteSpace(speechOptions.ExecutablePath) &&
        File.Exists(ResolveConfiguredPath(speechOptions.ExecutablePath));

    public async Task<string> TranscribeAsync(byte[] audioBytes, CancellationToken cancellationToken)
    {
        if (!IsConfigured)
        {
            return string.Empty;
        }

        if (audioBytes.Length == 0)
        {
            return string.Empty;
        }

        var inputExtension = string.IsNullOrWhiteSpace(speechOptions.InputFileExtension)
            ? ".webm"
            : speechOptions.InputFileExtension.StartsWith('.')
                ? speechOptions.InputFileExtension
                : $".{speechOptions.InputFileExtension}";

        var tempId = Guid.NewGuid().ToString("N");
        var tempPath = Path.Combine(Path.GetTempPath(), $"me-workspace-speech-{tempId}{inputExtension}");
        var outputBasePath = Path.Combine(Path.GetTempPath(), $"me-workspace-speech-{tempId}-output");
        var outputTextPath = $"{outputBasePath}.txt";

        try
        {
            await File.WriteAllBytesAsync(tempPath, audioBytes, cancellationToken);

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ResolveConfiguredPath(speechOptions.ExecutablePath),
                    Arguments = BuildArguments(tempPath, outputBasePath),
                    WorkingDirectory = ResolveWorkingDirectory(),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            process.Start();

            var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

            await process.WaitForExitAsync(cancellationToken);

            var stdout = (await stdoutTask).Trim();
            var stderr = (await stderrTask).Trim();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"Local speech engine exited with code {process.ExitCode}. {stderr}".Trim());
            }

            if (File.Exists(outputTextPath))
            {
                var fileTranscript = (await File.ReadAllTextAsync(outputTextPath, cancellationToken)).Trim();
                if (!string.IsNullOrWhiteSpace(fileTranscript))
                {
                    return fileTranscript;
                }
            }

            return !string.IsNullOrWhiteSpace(stdout)
                ? stdout
                : stderr;
        }
        finally
        {
            try
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }

                if (File.Exists(outputTextPath))
                {
                    File.Delete(outputTextPath);
                }
            }
            catch
            {
            }
        }
    }

    private string BuildArguments(string inputPath, string outputBasePath)
    {
        if (string.Equals(speechOptions.Provider, "WhisperCpp", StringComparison.OrdinalIgnoreCase))
        {
            return BuildWhisperCppArguments(inputPath, outputBasePath);
        }

        return BuildTemplateArguments(inputPath, outputBasePath);
    }

    private string BuildWhisperCppArguments(string inputPath, string outputBasePath)
    {
        if (string.IsNullOrWhiteSpace(speechOptions.ModelPath))
        {
            throw new InvalidOperationException("Speech:ModelPath must be set when Provider is WhisperCpp.");
        }

        var arguments = new List<string>
        {
            "-m", QuoteArgument(ResolveConfiguredPath(speechOptions.ModelPath)),
            "-f", QuoteArgument(inputPath),
            "-of", QuoteArgument(outputBasePath),
            "-otxt",
            "-l", QuoteArgument(speechOptions.Language),
        };

        if (speechOptions.NoTimestamps)
        {
            arguments.Add("-nt");
        }

        if (speechOptions.TranslateToEnglish)
        {
            arguments.Add("-tr");
        }

        if (speechOptions.Threads > 0)
        {
            arguments.Add("-t");
            arguments.Add(speechOptions.Threads.ToString());
        }

        return string.Join(" ", arguments);
    }

    private string BuildTemplateArguments(string inputPath, string outputBasePath)
    {
        var template = speechOptions.Arguments ?? string.Empty;
        return template
            .Replace("{input}", QuoteArgument(inputPath), StringComparison.OrdinalIgnoreCase)
            .Replace("{output}", QuoteArgument(outputBasePath), StringComparison.OrdinalIgnoreCase)
            .Replace("{model}", QuoteArgument(ResolveConfiguredPath(speechOptions.ModelPath)), StringComparison.OrdinalIgnoreCase);
    }

    private string ResolveWorkingDirectory() =>
        string.IsNullOrWhiteSpace(speechOptions.WorkingDirectory)
            ? AppContext.BaseDirectory
            : ResolveConfiguredPath(speechOptions.WorkingDirectory);

    private string ResolveConfiguredPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        return Path.IsPathRooted(path)
            ? path
            : Path.GetFullPath(path, contentRootPath);
    }

    private static string QuoteArgument(string value) =>
        $"\"{value.Replace("\"", "\\\"", StringComparison.Ordinal)}\"";
}
