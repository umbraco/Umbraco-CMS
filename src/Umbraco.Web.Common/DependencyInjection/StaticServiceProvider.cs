using System;
using System.ComponentModel;

namespace Umbraco.Cms.Web.Common.DependencyInjection
{
    /// <summary>
    /// INTERNAL Service locator. Should only be used if no other ways exist.
    /// </summary>
    /// <remarks>
    /// It is created with only two goals in mind
    /// 1) Continue to have the same extension methods on IPublishedContent and IPublishedElement as in V8. To make migration easier.
    /// 2) To have a tool to avoid breaking changes in minor versions. All methods using this should in theory be obsolete.
    ///
    /// Keep in mind, everything this is used, the code becomes basically untestable.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class StaticServiceProvider
    {
        /// <summary>
        /// The service locator.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static IServiceProvider Instance { get; set; }
    }
}
