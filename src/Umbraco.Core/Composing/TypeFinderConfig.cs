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

    public TypeFinderConfig(IOptions<TypeFinderSettings> settings) => _settings = settings.Value;

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
                ? Array.Empty<string>()
                : s.Split(',').Select(x => x.Trim()).ToArray();
        }
    }
}
