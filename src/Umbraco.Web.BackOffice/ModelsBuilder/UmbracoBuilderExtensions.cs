using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.DependencyInjection;

namespace Umbraco.Web.BackOffice.ModelsBuilder
{
    /// <summary>
    /// Extension methods for <see cref="IUmbracoBuilder"/> for the common Umbraco functionality
    /// </summary>
    public static class UmbracoBuilderExtensions
    {
        /// <summary>
        /// Can be called if using an external models builder to remove the embedded models builder controller features
        /// </summary>
        public static IUmbracoBuilder DisableModelsBuilderControllers(this IUmbracoBuilder builder)
        {
            builder.Services.AddSingleton<DisableModelsBuilderNotificationHandler>();
            return builder;
        }

    }
}
