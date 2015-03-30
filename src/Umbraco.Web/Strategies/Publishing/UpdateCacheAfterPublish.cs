using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Publishing;
using Umbraco.Web.Cache;

namespace Umbraco.Web.Strategies.Publishing
{
    [Obsolete("This is not used and will be removed from the codebase in future versions")]
    public class UpdateCacheAfterPublish : ApplicationEventHandler
    {
    }
}