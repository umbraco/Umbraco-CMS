using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web;
using Umbraco.Web.Models.Segments;

namespace Umbraco.Web.Routing.Segments
{
    /// <summary>
    /// Assigns segments based on MS's HttpBrowserCapabilities object
    /// </summary>
    [DisplayName("Browser capabilities provider")]
    [Description("Uses ASP.Net HttpBrowserCapabilities object to analyze the current request")]
    [ContentVariant("Mobile users", "IsMobileDevice")]
    [ContentVariant("Java applet people", "JavaApplets")]
    internal class BrowserCapabilitiesProvider : ContentSegmentProvider
    {
        public BrowserCapabilitiesProvider()
        {
            _browserCapabilityProps = typeof(HttpBrowserCapabilitiesBase).GetProperties()
                .Where(x => PropNames.Contains(x.Name))
                .ToArray();
        }

        private readonly PropertyInfo[] _browserCapabilityProps;

        private static readonly string[] PropNames =
        {
            "Browser",
            "IsMobileDevice",
            "JavaApplets",
            "MajorVersion",
            "MinorVersion",
            "MobileDeviceModel",
            "MobileDeviceManufacturer",
            "Platform"
        };

        public override SegmentCollection GetSegmentsForRequest(Uri originalRequestUrl, Uri cleanedRequestUrl, HttpRequestBase httpRequest)
        {
            return new SegmentCollection(
                _browserCapabilityProps
                    .Select(x => new Segment(x.Name, x.GetValue(httpRequest.Browser, null))));
        }
    }
}