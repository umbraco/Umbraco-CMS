using Smidge;
using Smidge.Nuglify;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace Umbraco.Extensions;

public static partial class UmbracoApplicationBuilderExtensions
{
    /// <summary>
    ///     Enables runtime minification for Umbraco
    /// </summary>
    public static IUmbracoEndpointBuilderContext UseUmbracoRuntimeMinificationEndpoints(
        this IUmbracoEndpointBuilderContext app)
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        if (!app.RuntimeState.UmbracoCanBoot())
        {
            return app;
        }

        app.AppBuilder.UseSmidge();
        app.AppBuilder.UseSmidgeNuglify();

        return app;
    }
}
