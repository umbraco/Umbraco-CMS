using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Configuration
{
    /// <summary>
    /// Configures named options for the <see cref="UmbracoConnectionString" /> model and ensures the provider name is always set.
    /// </summary>
    /// <seealso cref="IPostConfigureOptions{UmbracoConnectionString}" />
    public class PostConfigureUmbracoConnectionStringOptions : IPostConfigureOptions<UmbracoConnectionString>
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostConfigureUmbracoConnectionStringOptions"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public PostConfigureUmbracoConnectionStringOptions(IConfiguration configuration)
            => _configuration = configuration;

        /// <inheritdoc />
        public void PostConfigure(string name, UmbracoConnectionString options)
        {
            if (!string.IsNullOrEmpty(options.ConnectionString))
            {
                // Ensure the provider name is set
                if (string.IsNullOrEmpty(options.ProviderName))
                {
                    options.ProviderName = UmbracoConnectionString.ParseProviderName(options.ConnectionString);
                }
            }
            else
            {
                // Automatically populate values from configuation
                if (name == Options.DefaultName)
                {
                    options.ConnectionString = _configuration.GetUmbracoConnectionString(out string providerName);
                    options.ProviderName = providerName;
                }
                else
                {
                    options.ConnectionString = _configuration.GetUmbracoConnectionString(name, out string providerName);
                    options.ProviderName = providerName;
                }
            }
        }
    }
}
