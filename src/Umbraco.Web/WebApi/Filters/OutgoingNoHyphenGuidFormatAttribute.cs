using System;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;

namespace Umbraco.Web.WebApi.Filters
{
    internal sealed class OutgoingNoHyphenGuidFormatAttribute : Attribute, IControllerConfiguration
    {
        public OutgoingNoHyphenGuidFormatAttribute()
        {
        }

        public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            var jsonFormatter = controllerSettings.Formatters.OfType<JsonMediaTypeFormatter>();
            foreach (var r in jsonFormatter)
            {
                r.SerializerSettings.Converters.Add(new GuidNoHyphenConverter());
            }
        }

    }
}