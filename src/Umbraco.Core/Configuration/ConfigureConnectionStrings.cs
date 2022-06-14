using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Configuration;

/// <summary>
///     Configures the <see cref="ConnectionStrings" /> named option.
/// </summary>
public class ConfigureConnectionStrings : IConfigureNamedOptions<ConnectionStrings>
{
    private readonly IConfiguration _configuration;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigureConnectionStrings" /> class.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    public ConfigureConnectionStrings(IConfiguration configuration)
        => _configuration = configuration;

    /// <inheritdoc />
    public void Configure(ConnectionStrings options) => Configure(Options.DefaultName, options);

    /// <inheritdoc />
    public void Configure(string name, ConnectionStrings options)
    {
        // Default to using UmbracoConnectionName
        if (name == Options.DefaultName)
        {
            name = Constants.System.UmbracoConnectionName;
        }

        if (options.IsConnectionStringConfigured())
        {
            return;
        }

        options.Name = name;
        options.ConnectionString = _configuration.GetUmbracoConnectionString(name, out string? providerName);
        options.ProviderName = providerName ??
                               ConnectionStrings.DefaultProviderName;
    }
}
