using System;
using System.ComponentModel;
using System.Web;

namespace Umbraco.Web.Routing.Segments
{
    /// <summary>
    /// A configurable provider to match against the referrer
    /// </summary>
    [DisplayName("Referrer provider")]
    [Description("A configurable provider that analyzes the current request's referrer")]
    public class ReferrerSegmentProvider : ConfigurableSegmentProvider
    {
        public override object GetCurrentValue(Uri cleanedRequestUrl, HttpRequestBase httpRequest)
        {
            return httpRequest.UrlReferrer == null ? "" : httpRequest.UrlReferrer.OriginalString;
        }
    }
}