using System;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;

namespace Umbraco.Web.WebApi
{
    /// <summary>
    /// Applying this attribute to any webapi controller will ensure that it only contains one json formatter compatible with the angular json vulnerability prevention.
    /// </summary>
    public class AngularJsonOnlyConfigurationAttribute : Attribute, IControllerConfiguration
    {
        public virtual void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {            
            //remove all json/xml formatters then add our custom one
            var toRemove = controllerSettings.Formatters.Where(t => (t is JsonMediaTypeFormatter) || (t is XmlMediaTypeFormatter)).ToList();
            foreach (var r in toRemove)
            {
                controllerSettings.Formatters.Remove(r);
            }
            controllerSettings.Formatters.Add(new AngularJsonMediaTypeFormatter());
        }
    }
}