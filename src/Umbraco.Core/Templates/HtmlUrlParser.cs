using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Logging;

namespace Umbraco.Cms.Core.Templates;

public sealed class HtmlUrlParser
{
    private static readonly Regex ResolveUrlPattern = new(
        "(=[\"\']?)(\\W?\\~(?:.(?![\"\']?\\s+(?:\\S+)=|[>\"\']))+.)[\"\']?",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

    private readonly IIOHelper _ioHelper;
    private readonly ILogger<HtmlUrlParser> _logger;
    private readonly IProfilingLogger _profilingLogger;
    private ContentSettings _contentSettings;

    public HtmlUrlParser(IOptionsMonitor<ContentSettings> contentSettings, ILogger<HtmlUrlParser> logger, IProfilingLogger profilingLogger, IIOHelper ioHelper)
    {
        _contentSettings = contentSettings.CurrentValue;
        _logger = logger;
        _ioHelper = ioHelper;
        _profilingLogger = profilingLogger;

        contentSettings.OnChange(x => _contentSettings = x);
    }

    /// <summary>
    ///     The RegEx matches any HTML attribute values that start with a tilde (~), those that match are passed to ResolveUrl
    ///     to replace the tilde with the application path.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    /// <remarks>
    ///     When used with a Virtual-Directory set-up, this would resolve all URLs correctly.
    ///     The recommendation is that the "ResolveUrlsFromTextString" option (in umbracoSettings.config) is set to false for
    ///     non-Virtual-Directory installs.
    /// </remarks>
    public string EnsureUrls(string text)
    {
        if (_contentSettings.ResolveUrlsFromTextString == false)
        {
            return text;
        }

        using (DisposableTimer? timer = _profilingLogger.DebugDuration(
                   typeof(IOHelper),
                   "ResolveUrlsFromTextString starting",
                   "ResolveUrlsFromTextString complete"))
        {
            // find all relative URLs (ie. URLs that contain ~)
            MatchCollection tags = ResolveUrlPattern.Matches(text);
            _logger.LogDebug("After regex: {Duration} matched: {TagsCount}", timer?.Stopwatch.ElapsedMilliseconds, tags.Count);
            foreach (Match tag in tags)
            {
                var url = string.Empty;
                if (tag.Groups[1].Success)
                {
                    url = tag.Groups[1].Value;
                }

                // The richtext editor inserts a slash in front of the URL. That's why we need this little fix
                //                if (url.StartsWith("/"))
                //                    text = text.Replace(url, ResolveUrl(url.Substring(1)));
                //                else
                if (string.IsNullOrEmpty(url) == false)
                {
                    var resolvedUrl = url[..1] == "/"
                        ? _ioHelper.ResolveUrl(url[1..])
                        : _ioHelper.ResolveUrl(url);
                    text = text.Replace(url, resolvedUrl);
                }
            }
        }

        return text;
    }
}
