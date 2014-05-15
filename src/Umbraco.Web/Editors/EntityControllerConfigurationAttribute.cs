using System;
using System.Web.Http.Controllers;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// This get's applied to the EntityController in order to have a custom IHttpActionSelector assigned to it
    /// </summary>
    /// <remarks>
    /// NOTE: It is SOOOO important to remember that you cannot just assign this in the 'initialize' method of a webapi
    /// controller as it will assign it GLOBALLY which is what you def do not want to do.
    /// </remarks>
    internal class EntityControllerConfigurationAttribute : Attribute, IControllerConfiguration
    {
        public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            controllerSettings.Services.Replace(typeof(IHttpActionSelector), new EntityControllerActionSelector());
        }
    }
}