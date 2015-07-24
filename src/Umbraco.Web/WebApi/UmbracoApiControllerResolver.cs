using System;
using System.Collections.Generic;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Web.WebApi
{
    internal sealed class UmbracoApiControllerResolver : ManyObjectsResolverBase<UmbracoApiControllerResolver, UmbracoApiController>
    {
        public UmbracoApiControllerResolver(IServiceProvider serviceProvider, ILogger logger, IEnumerable<Type> apiControllers)
            : base(serviceProvider, logger, apiControllers)
        {

        }
        
        /// <summary>
        /// Gets all of the umbraco api controller types
        /// </summary>
        public IEnumerable<Type> RegisteredUmbracoApiControllers
        {
            get { return InstanceTypes; }
        }

    }
}