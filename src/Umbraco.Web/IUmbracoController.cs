using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.Security;

namespace Umbraco.Web
{
    /// <summary>
    /// Common properties of controllers of different type. 
    /// This serves no purpose within the Core (at the time of writing) but is useful for writing extension methods
    /// </summary>
    public interface IUmbracoController
    {
        /// <summary>
        /// Returns an ILogger
        /// </summary>
        ILogger Logger { get; }

        /// <summary>
        /// Returns a ProfilingLogger
        /// </summary>
        ProfilingLogger ProfilingLogger { get; }

        /// <summary>
        /// Returns the current ApplicationContext
        /// </summary>
        ApplicationContext ApplicationContext { get; }

        /// <summary>
        /// Returns a ServiceContext
        /// </summary>
        ServiceContext Services { get; }

        /// <summary>
        /// Returns a DatabaseContext
        /// </summary>
        DatabaseContext DatabaseContext { get; }

        /// <summary>
        /// Returns an UmbracoHelper object
        /// </summary>
        UmbracoHelper Umbraco { get; }

        /// <summary>
        /// Returns the current UmbracoContext
        /// </summary>
        UmbracoContext UmbracoContext { get; }

        /// <summary>
        /// Returns the MemberHelper instance
        /// </summary>
        MembershipHelper Members { get; }
    }
}