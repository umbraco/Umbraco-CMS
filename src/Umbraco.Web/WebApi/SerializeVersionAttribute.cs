using Newtonsoft.Json.Converters;
using System;
using System.Web.Http.Controllers;

namespace Umbraco.Web.WebApi
{
    internal class SerializeVersionAttribute : Attribute, IControllerConfiguration
    {
        public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            var formatter = controllerSettings.Formatters.JsonFormatter;
            formatter.SerializerSettings.Converters.Add(new VersionConverter());
        }
    }
}
