using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Auditing;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Web.Strategies
{
    [Obsolete("This class is no longer used and will be removed from the codebase in future versions")]
    public sealed class RelateOnCopyHandler : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {            
        }
    }
}
