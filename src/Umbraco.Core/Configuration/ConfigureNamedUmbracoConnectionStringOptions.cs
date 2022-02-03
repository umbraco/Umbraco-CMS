using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Configuration
{
    /// <summary>
    /// Configures named options for the <see cref="UmbracoConnectionString" /> model.
    /// </summary>
    /// <seealso cref="IConfigureNamedOptions{UmbracoConnectionString}" />
    public class ConfigureNamedUmbracoConnectionStringOptions : IConfigureNamedOptions<UmbracoConnectionString>
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureNamedUmbracoConnectionStringOptions"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public ConfigureNamedUmbracoConnectionStringOptions(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <inheritdoc />
        public void Configure(string name, UmbracoConnectionString options)
        {
            if (options.IsConnectionStringConfigured())
            {
                // Skip if already configured
                return;
            }

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

        /// <inheritdoc />
        public void Configure(UmbracoConnectionString options)
        {
        }
    }
}
