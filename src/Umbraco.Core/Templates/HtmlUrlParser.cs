using System.Text.RegularExpressions;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

namespace Umbraco.Web.Templates
{
    public sealed class HtmlUrlParser
    {
        private readonly IContentSettings _contentSettings;
        private readonly IIOHelper _ioHelper;
        private readonly IProfilingLogger _logger;

        private static readonly Regex ResolveUrlPattern = new Regex("(=[\"\']?)(\\W?\\~(?:.(?![\"\']?\\s+(?:\\S+)=|[>\"\']))+.)[\"\']?",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        public HtmlUrlParser(IContentSettings contentSettings, IProfilingLogger logger, IIOHelper ioHelper)
        {
            _contentSettings = contentSettings;
            _ioHelper = ioHelper;
            _logger = logger;
        }

        /// <summary>
        /// The RegEx matches any HTML attribute values that start with a tilde (~), those that match are passed to ResolveUrl to replace the tilde with the application path.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        /// <remarks>
        /// When used with a Virtual-Directory set-up, this would resolve all URLs correctly.
        /// The recommendation is that the "ResolveUrlsFromTextString" option (in umbracoSettings.config) is set to false for non-Virtual-Directory installs.
        /// </remarks>
        public string EnsureUrls(string text)
        {
            if (_contentSettings.ResolveUrlsFromTextString == false) return text;

            using (var timer = _logger.DebugDuration(typeof(IOHelper), "ResolveUrlsFromTextString starting", "ResolveUrlsFromTextString complete"))
            {
                // find all relative URLs (ie. URLs that contain ~)
                var tags = ResolveUrlPattern.Matches(text);
                _logger.Debug(typeof(IOHelper), "After regex: {Duration} matched: {TagsCount}", timer.Stopwatch.ElapsedMilliseconds, tags.Count);
                foreach (Match tag in tags)
                {
                    var url = "";
                    if (tag.Groups[1].Success)
                        url = tag.Groups[1].Value;

                    // The richtext editor inserts a slash in front of the URL. That's why we need this little fix
                    //                if (url.StartsWith("/"))
                    //                    text = text.Replace(url, ResolveUrl(url.Substring(1)));
                    //                else
                    if (string.IsNullOrEmpty(url) == false)
                    {
                        var resolvedUrl = (url.Substring(0, 1) == "/") ? _ioHelper.ResolveUrl(url.Substring(1)) : _ioHelper.ResolveUrl(url);
                        text = text.Replace(url, resolvedUrl);
                    }
                }
            }

            return text;
        }
    }
}
