using System;
using Umbraco.Core;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Web.Strategies.Publishing
{
    [Obsolete("This is not used and will be removed from the codebase in future versions")]
    [Weight(-100)]
    public class UpdateCacheAfterPublish : ApplicationEventHandler
    {
    }

    [Obsolete("This is not used and will be removed from the codebase in future versions")]
    [Weight(-100)]
    public class UpdateCacheAfterUnPublish : ApplicationEventHandler
    {
    }
}
