using System;
using System.Collections.Generic;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Routing
{
    public interface IContextUrlProvider
    {
        UrlMode Mode { get; set; }

        string GetMediaUrl(Guid id, UrlMode mode = UrlMode.Default, string culture = null, string propertyAlias = "umbracoFile", Uri current = null);
        string GetMediaUrl(IPublishedContent content, UrlMode mode = UrlMode.Default, string culture = null, string propertyAlias = "umbracoFile", Uri current = null);
        IEnumerable<UrlInfo> GetOtherUrls(int id);
        IEnumerable<UrlInfo> GetOtherUrls(int id, Uri current);
        string GetUrl(Guid id, UrlMode mode = UrlMode.Default, string culture = null, Uri current = null);
        string GetUrl(int id, UrlMode mode = UrlMode.Default, string culture = null, Uri current = null);
        string GetUrl(IPublishedContent content, UrlMode mode = UrlMode.Default, string culture = null, Uri current = null);
    }
}