using System;
using System.Web.Http.Controllers;
using Newtonsoft.Json.Serialization;

namespace Umbraco.Web.WebApi
{
    /// <summary>
    /// Applying this attribute to any webapi controller will ensure that it only contains one json formatter compatible with the angular json vulnerability prevention.
    /// </summary>
    public class JsonCamelCaseFormatter : Attribute, IControllerConfiguration
    {
        public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            controllerSettings.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }    
    }
}