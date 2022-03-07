using System;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Dashboards;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.ModelsBuilder;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.ModelsBuilder
{
    /// <summary>
    /// Extension methods for <see cref="IUmbracoBuilder"/> for the common Umbraco functionality
    /// </summary>
    public static class UmbracoBuilderExtensions
    {
        /// <summary>
        /// Adds the ModelsBuilder dashboard, but only when not in production mode.
        /// </summary>
        internal static IUmbracoBuilder TryAddModelsBuilderDashboard(this IUmbracoBuilder builder)
        {
            if (builder.Config.IsRuntimeMode(RuntimeMode.Production))
            {
                builder.RemoveModelsBuilderDashboard();
            }
            else
            {
                builder.AddModelsBuilderDashboard();
            }

            return builder;
        }

        /// <summary>
        /// Adds the ModelsBuilder dashboard (dashboard and API controller are automatically added).
        /// </summary>
        public static IUmbracoBuilder AddModelsBuilderDashboard(this IUmbracoBuilder builder)
        {
            builder.Services.AddUnique<IModelsBuilderDashboardProvider, ModelsBuilderDashboardProvider>();

            return builder;
        }

        /// <summary>
        /// Removes the ModelsBuilder dashboard (and API controller).
        /// </summary>
        public static IUmbracoBuilder RemoveModelsBuilderDashboard(this IUmbracoBuilder builder)
        {
            builder.Dashboards().Remove<ModelsBuilderDashboard>();
            builder.WithCollectionBuilder<UmbracoApiControllerTypeCollectionBuilder>().Remove<ModelsBuilderDashboardController>();

            return builder;
        }

        /// <summary>
        /// Can be called if using an external models builder to remove the embedded models builder controller features
        /// </summary>
        [Obsolete("This doesn't remove the controller or dashboard, use RemoveModelsBuilderDashboard() instead.")]
        public static IUmbracoBuilder DisableModelsBuilderControllers(this IUmbracoBuilder builder)
        {
            builder.Services.AddSingleton<DisableModelsBuilderNotificationHandler>();
            return builder;
        }
    }
}
