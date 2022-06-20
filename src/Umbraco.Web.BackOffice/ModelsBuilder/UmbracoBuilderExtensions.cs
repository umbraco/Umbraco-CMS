using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.ModelsBuilder;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.ModelsBuilder;

/// <summary>
///     Extension methods for <see cref="IUmbracoBuilder" /> for the common Umbraco functionality
/// </summary>
public static class UmbracoBuilderExtensions
{
    /// <summary>
    ///     Adds the ModelsBuilder dashboard.
    /// </summary>
    public static IUmbracoBuilder AddModelsBuilderDashboard(this IUmbracoBuilder builder)
    {
        builder.Services.AddUnique<IModelsBuilderDashboardProvider, ModelsBuilderDashboardProvider>();
        return builder;
    }

    /// <summary>
    ///     Can be called if using an external models builder to remove the embedded models builder controller features
    /// </summary>
    public static IUmbracoBuilder DisableModelsBuilderControllers(this IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<DisableModelsBuilderNotificationHandler>();
        return builder;
    }
}
