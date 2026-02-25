using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.ModelsBuilder;

/// <summary>
/// Represents an error that occurs during model generation in the ModelsBuilder.
/// </summary>
public sealed class ModelsGenerationError
{
    private readonly IHostEnvironment _hostEnvironment;
    private ModelsBuilderSettings _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.ModelsBuilder.ModelsGenerationError"/> class with the specified configuration and hosting environment.
    /// </summary>
    /// <param name="config">An <see cref="IOptionsMonitor{ModelsBuilderSettings}"/> providing access to ModelsBuilder configuration settings.</param>
    /// <param name="hostEnvironment">An <see cref="IHostEnvironment"/> representing the current hosting environment.</param>
    public ModelsGenerationError(IOptionsMonitor<ModelsBuilderSettings> config, IHostEnvironment hostEnvironment)
    {
        _config = config.CurrentValue;
        _hostEnvironment = hostEnvironment;
        config.OnChange(x => _config = x);
    }

    /// <summary>
    /// Deletes the models generation error file if it exists, effectively clearing any recorded error.
    /// </summary>
    public void Clear()
    {
        var errFile = GetErrFile();
        if (errFile == null)
        {
            return;
        }

        // "If the file to be deleted does not exist, no exception is thrown."
        File.Delete(errFile);
    }

    /// <summary>
    /// Reports a models generation error by writing the specified error message and exception details to an error file.
    /// If the error file cannot be obtained, the method returns without performing any action.
    /// </summary>
    /// <param name="message">The error message describing the models generation error.</param>
    /// <param name="e">The <see cref="Exception"/> instance containing details about the error.</param>
    public void Report(string message, Exception e)
    {
        var errFile = GetErrFile();
        if (errFile == null)
        {
            return;
        }

        var sb = new StringBuilder();
        sb.Append(message);
        sb.Append("\r\n");
        sb.Append(e.Message);
        sb.Append("\r\n\r\n");
        sb.Append(e.StackTrace);
        sb.Append("\r\n");

        File.WriteAllText(errFile, sb.ToString());
    }

    /// <summary>
    /// Retrieves the last error message recorded in the error file, if available.
    /// </summary>
    /// <returns>
    /// The last error message as a string, or <c>null</c> if the error file does not exist or cannot be read.
    /// </returns>
    public string? GetLastError()
    {
        var errFile = GetErrFile();
        if (errFile == null)
        {
            return null;
        }

        try
        {
            return File.ReadAllText(errFile);
        }
        catch
        {
            // accepted
            return null;
        }
    }

    private string? GetErrFile()
    {
        var modelsDirectory = _config.ModelsDirectoryAbsolute(_hostEnvironment);
        if (!Directory.Exists(modelsDirectory))
        {
            return null;
        }

        return Path.Combine(modelsDirectory, "models.err");
    }
}
