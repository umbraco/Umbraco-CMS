using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;
using Umbraco.Web.PropertyEditors;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using File = System.IO.File;

namespace Umbraco.Web.Templates
{

    [Obsolete("This class is obsolete, all methods have been moved to other classes: " + nameof(HtmlLocalLinkParser) + ", " + nameof(HtmlUrlParser) + " and " + nameof(HtmlImageSourceParser))]
    public static class TemplateUtilities
    {

        // TODO: Replace mediaCache with media url provider
        internal static string ParseInternalLinks(string text, UrlProvider urlProvider, IPublishedMediaCache mediaCache)
        {
            if (urlProvider == null) throw new ArgumentNullException(nameof(urlProvider));
            if (mediaCache == null) throw new ArgumentNullException(nameof(mediaCache));

            // Parse internal links
            var tags = HtmlLocalLinkParser.LocalLinkPattern.Matches(text);
            foreach (Match tag in tags)
            {
                if (tag.Groups.Count > 0)
                {
                    var id = tag.Groups[1].Value; //.Remove(tag.Groups[1].Value.Length - 1, 1);

                    //The id could be an int or a UDI
                    if (UdiParser.TryParse(id, out var udi))
                    {
                        var guidUdi = udi as GuidUdi;
                        if (guidUdi != null)
                        {
                            var newLink = "#";
                            if (guidUdi.EntityType == Constants.UdiEntityType.Document)
                                newLink = urlProvider.GetUrl(guidUdi.Guid);
                            else if (guidUdi.EntityType == Constants.UdiEntityType.Media)
                                newLink = mediaCache.GetById(guidUdi.Guid)?.Url;

                            if (newLink == null)
                                newLink = "#";

                            text = text.Replace(tag.Value, "href=\"" + newLink);
                        }
                    }

                    if (int.TryParse(id, out var intId))
                    {
                        var newLink = urlProvider.GetUrl(intId);
                        text = text.Replace(tag.Value, "href=\"" + newLink);
                    }
                }
            }

            return text;
        }


    }
}
