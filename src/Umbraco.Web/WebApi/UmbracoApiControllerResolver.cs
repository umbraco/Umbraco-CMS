using System;
using System.Collections.Generic;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Web.WebApi
{
    internal sealed class UmbracoApiControllerResolver : ManyObjectsResolverBase<UmbracoApiControllerResolver, UmbracoApiController>
    {
        public UmbracoApiControllerResolver(IEnumerable<Type> apiControllers)
            : base(apiControllers)
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