using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;

namespace Umbraco.Cms.Core.Strings;

/// <summary>
/// Static wrapper for backward compatibility with existing code.
/// </summary>
/// <remarks>
/// Use <see cref="IUtf8ToAsciiConverter"/> via dependency injection for new code.
/// </remarks>
public static class Utf8ToAsciiConverterStatic
{
    private static readonly Lazy<IUtf8ToAsciiConverter> DefaultConverter = new(() =>
    {
        var hostEnv = new SimpleHostEnvironment { ContentRootPath = AppContext.BaseDirectory };
        var loader = new CharacterMappingLoader(hostEnv, NullLogger<CharacterMappingLoader>.Instance);
        return new Utf8ToAsciiConverter(loader);
    });

    /// <summary>
    /// Gets the default converter instance for use in tests and other scenarios where DI is not available.
    /// </summary>
    internal static IUtf8ToAsciiConverter Instance => DefaultConverter.Value;

    // Simple IHostEnvironment implementation for static initialization
    private sealed class SimpleHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Production";
        public string ApplicationName { get; set; } = "Umbraco";
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }

    /// <summary>
    /// Converts an UTF-8 string into an ASCII string.
    /// </summary>
    /// <param name="text">The text to convert.</param>
    /// <param name="fail">The character to use to replace characters that cannot be converted.</param>
    /// <returns>The converted text.</returns>
    [Obsolete("Use IUtf8ToAsciiConverter via dependency injection. This will be removed in v15.")]
    public static string ToAsciiString(string text, char fail = '?')
        => DefaultConverter.Value.Convert(text, fail);

    /// <summary>
    /// Converts an UTF-8 string into an array of ASCII characters.
    /// </summary>
    /// <param name="text">The text to convert.</param>
    /// <param name="fail">The character to use to replace characters that cannot be converted.</param>
    /// <returns>The converted text as char array.</returns>
    [Obsolete("Use IUtf8ToAsciiConverter via dependency injection. This will be removed in v15.")]
    public static char[] ToAsciiCharArray(string text, char fail = '?')
        => DefaultConverter.Value.Convert(text, fail).ToCharArray();
}
