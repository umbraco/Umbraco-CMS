using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Configuration.UmbracoSettings;

namespace Umbraco.Cms.Core.Composing;

/// <summary>
///     TypeFinder config via appSettings
/// </summary>
public class TypeFinderConfig : ITypeFinderConfig
{
    private readonly TypeFinderSettings _settings;
    private IEnumerable<string>? _assembliesAcceptingLoadExceptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeFinderConfig" /> class.
    /// </summary>
    /// <param name="settings">The type finder settings options.</param>
    public TypeFinderConfig(IOptions<TypeFinderSettings> settings) => _settings = settings.Value;

    /// <inheritdoc />
    public IEnumerable<string> AssembliesAcceptingLoadExceptions
    {
        get
        {
            if (_assembliesAcceptingLoadExceptions != null)
            {
                return _assembliesAcceptingLoadExceptions;
            }

            var s = _settings.AssembliesAcceptingLoadExceptions;
            return _assembliesAcceptingLoadExceptions = string.IsNullOrWhiteSpace(s)
                ? []
                : s.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
