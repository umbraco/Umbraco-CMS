using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Filters;
using Umbraco.Core;
using Umbraco.Core.Logging;

namespace Umbraco.Web.WebApi
{
    /// <summary>
    /// Adds our unhandled exception logger to the controller's services
    /// </summary>
    /// <remarks>
    /// Important to note that the <see cref="UnhandledExceptionLogger"/> will only be called if the controller has an ExceptionFilter applied
    /// to it, so to kill two birds with one stone, this class inherits from ExceptionFilterAttribute purely to force webapi to use the 
    /// IExceptionLogger (strange)
    /// </remarks>
    public class UnhandedExceptionLoggerConfigurationAttribute : ExceptionFilterAttribute, IControllerConfiguration
    {
        public virtual void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            controllerSettings.Services.Add(typeof(IExceptionLogger), new UnhandledExceptionLogger());
        }
        
    }
}