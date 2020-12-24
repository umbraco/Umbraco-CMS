using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Composing;
using Umbraco.Core.Diagnostics;
using Umbraco.Core.Hosting;
using Umbraco.Core.Logging;
using Umbraco.Core.Security;
using Umbraco.Extensions;
using Umbraco.Net;
using Umbraco.Web.Common.AspNetCore;
using Umbraco.Web.Common.Controllers;
using Umbraco.Web.Common.Install;
using Umbraco.Web.Common.Lifetime;
using Umbraco.Web.Common.Macros;
using Umbraco.Web.Common.Middleware;
using Umbraco.Web.Common.Profiler;
using Umbraco.Web.Common.Routing;
using Umbraco.Web.Common.Security;
using Umbraco.Web.Common.Templates;
using Umbraco.Web.Macros;
using Umbraco.Web.Security;
using Umbraco.Web.Templates;
using Umbraco.Web.Common.ModelBinders;
using Umbraco.Infrastructure.DependencyInjection;

namespace Umbraco.Web.Common.Runtime
{

    /// <summary>
    /// Adds/replaces AspNetCore specific services
    /// </summary>
    [ComposeBefore(typeof(ICoreComposer))]
    public class AspNetCoreComposer : ComponentComposer<AspNetCoreComponent>, IComposer
    {
    }
}
