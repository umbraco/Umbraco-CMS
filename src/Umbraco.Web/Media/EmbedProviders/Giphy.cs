﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Media.EmbedProviders
{
    /// <summary>
    /// Embed Provider for Giphy.com the popular online GIFs and animated sticker provider.
    /// </summary>
    public class Giphy : EmbedProviderBase
    {
        public override string ApiEndpoint => "https://giphy.com/services/oembed?url=";

        public override string[] UrlSchemeRegex => new string[]
        {
            @"giphy\.com/*",
            @"gph\.is/*"
        };

        public override Dictionary<string, string> RequestParams => new Dictionary<string, string>();

        public override string GetMarkup(string url, int maxWidth = 0, int maxHeight = 0)
        {
            var requestUrl = base.GetEmbedProviderUrl(url, maxWidth, maxHeight);
            var oembed = base.GetJsonResponse<OEmbedResponse>(requestUrl);

            return oembed.GetHtml();
        }
    }
}
