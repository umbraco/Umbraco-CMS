using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Web.Middleware;

namespace Umbraco.Cms.Web.Common.DependencyInjection
{
    /// <summary>
    /// Configures the ImageSharp middleware options to use the registered configuration.
    /// </summary>
    /// <seealso cref="IConfigureOptions{ImageSharpMiddlewareOptions}" />
    public sealed class ImageSharpConfigurationOptions : IConfigureOptions<ImageSharpMiddlewareOptions>
    {
        /// <summary>
        /// The ImageSharp configuration.
        /// </summary>
        private readonly Configuration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageSharpConfigurationOptions" /> class.
        /// </summary>
        /// <param name="configuration">The ImageSharp configuration.</param>
        public ImageSharpConfigurationOptions(Configuration configuration) => _configuration = configuration;

        /// <summary>
        /// Invoked to configure an <see cref="ImageSharpMiddlewareOptions" /> instance.
        /// </summary>
        /// <param name="options">The options instance to configure.</param>
        public void Configure(ImageSharpMiddlewareOptions options) => options.Configuration = _configuration;
    }
}
