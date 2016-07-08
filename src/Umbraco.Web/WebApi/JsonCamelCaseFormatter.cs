using System;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using Newtonsoft.Json;
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
            //remove all json formatters then add our custom one
            var toRemove = controllerSettings.Formatters.Where(t => (t is JsonMediaTypeFormatter)).ToList();
            foreach (var r in toRemove)
            {
                controllerSettings.Formatters.Remove(r);
            }

            var jsonFormatter = new JsonMediaTypeFormatter
            {
                SerializerSettings =
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }
            };
            controllerSettings.Formatters.Add(jsonFormatter);
        }    
    }
}