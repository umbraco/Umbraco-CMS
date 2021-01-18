using System;
using System.Web;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Web.Composing;
using Umbraco.Web.Routing;

namespace Umbraco.Web
{
    /// <summary>
    /// Represents the main Umbraco module.
    /// </summary>
    /// <remarks>
    /// <para>Register that one in web.config.</para>
    /// <para>It will inject <see cref="UmbracoInjectedModule"/> which contains most of the actual code.</para>
    /// </remarks>
    public class UmbracoModule : ModuleInjector<UmbracoInjectedModule>
    {
        /// <summary>
        /// Occurs when...
        /// </summary>
        internal static event EventHandler<RoutableAttemptEventArgs> RouteAttempt;

        /// <summary>
        /// Occurs when...
        /// </summary>
        public static event EventHandler<UmbracoRequestEventArgs> EndRequest;

        /// <summary>
        /// Triggers the RouteAttempt event.
        /// </summary>
        internal static void OnRouteAttempt(object sender, RoutableAttemptEventArgs args)
        {
            RouteAttempt?.Invoke(sender, args);
        }

        /// <summary>
        /// Triggers the EndRequest event.
        /// </summary>
        internal static void OnEndRequest(object sender, UmbracoRequestEventArgs args)
        {
            EndRequest?.Invoke(sender, args);
        }
    }
}
