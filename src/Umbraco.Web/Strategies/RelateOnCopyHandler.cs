using System;
using Umbraco.Core;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Web.Strategies
{
    [Obsolete("This class is no longer used and will be removed from the codebase in future versions")]
    [Weight(-100)]
    public sealed class RelateOnCopyHandler : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {            
        }
    }
}
