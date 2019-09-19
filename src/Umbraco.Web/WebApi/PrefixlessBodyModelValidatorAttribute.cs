using System;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Validation;

namespace Umbraco.Web.WebApi
{
    /// <summary>
    /// Applying this attribute to any webapi controller will ensure that the <see cref="IBodyModelValidator"/> is of type <see cref="PrefixlessBodyModelValidator"/>
    /// </summary>
    internal class PrefixlessBodyModelValidatorAttribute : Attribute, IControllerConfiguration
    {
        public virtual void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            //replace the normal validator with our custom one for this controller
            controllerSettings.Services.Replace(typeof(IBodyModelValidator), 
                new PrefixlessBodyModelValidator(controllerSettings.Services.GetBodyModelValidator()));            
        }
    }
}