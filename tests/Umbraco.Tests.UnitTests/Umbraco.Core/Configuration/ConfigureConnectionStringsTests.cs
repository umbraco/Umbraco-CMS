using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Tests.UnitTests.AutoFixture;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Configuration;

/// <summary>
/// Contains unit tests for the <see cref="ConfigureConnectionStrings"/> class, verifying its behavior and configuration logic.
/// </summary>
[TestFixture]
public class ConfigureConnectionStringsTests
{
    private const string UmbracoDbDsn = Constants.System.UmbracoConnectionName;

    private IOptionsSnapshot<ConnectionStrings> GetOptions(IDictionary<string, string> configValues = null)
    {
        var configurationBuilder = new ConfigurationBuilder();
        if (configValues != null)
        {
            configurationBuilder.AddInMemoryCollection(configValues);
        }

        var configuration = configurationBuilder.Build();

        var services = new ServiceCollection();
        services.AddOptions();
        services.AddSingleton<IConfigureOptions<ConnectionStrings>, ConfigureConnectionStrings>();
        services.AddSingleton<IConfiguration>(configuration);

        var container = services.BuildServiceProvider();
        return container.GetRequiredService<IOptionsSnapshot<ConnectionStrings>>();
    }

    /// <summary>
    /// Tests that when the configuration is missing the provider name, the default provider name is set.
    /// </summary>
    [Test]
    public void Configure_WithConfigMissingProvider_SetsDefaultValue()
    {
        var result = GetOptions();
        Assert.Multiple(() =>
        {
            Assert.That(result.Value.ProviderName, Is.Not.Null);
            Assert.That(result.Value.ProviderName, Is.EqualTo(ConnectionStrings.DefaultProviderName));

            Assert.That(result.Get(UmbracoDbDsn).ProviderName, Is.Not.Null);
            Assert.That(result.Get(UmbracoDbDsn).ProviderName, Is.EqualTo(ConnectionStrings.DefaultProviderName));
        });
    }

    /// <summary>
    /// Verifies that when a connection string is configured with a specific provider name, the provider value is correctly respected and applied.
    /// </summary>
    /// <param name="aConnectionString">The connection string to be configured.</param>
    /// <param name="aProviderName">The provider name to be set and validated.</param>
    [Test]
    [AutoMoqData]
    public void Configure_WithConfiguredProvider_RespectsProviderValue(
        string aConnectionString,
        string aProviderName)
    {
        var config = new Dictionary<string, string>
        {
            [$"ConnectionStrings:{UmbracoDbDsn}"] = aConnectionString,
            [$"ConnectionStrings:{UmbracoDbDsn}_ProviderName"] = aProviderName,
        };

        var result = GetOptions(config);

        Assert.Multiple(() =>
        {
            Assert.That(result.Value.ProviderName, Is.Not.Null);
            Assert.That(result.Value.ProviderName, Is.EqualTo(aProviderName));

            Assert.That(result.Get(UmbracoDbDsn).ProviderName, Is.Not.Null);
            Assert.That(result.Get(UmbracoDbDsn).ProviderName, Is.EqualTo(aProviderName));
        });
    }

    /// <summary>
    /// Verifies that when the connection string configuration contains the DataDirectory placeholder,
    /// it is replaced with the specified data directory path.
    /// </summary>
    /// <param name="aDataDirectory">The path that should replace the DataDirectory placeholder in the connection string.</param>
    /// <param name="aConnectionString">The connection string segment to append after the data directory path.</param>
    /// <param name="aProviderName">The provider name for the connection string.</param>
    [Test]
    [AutoMoqData]
    public void Configure_WithDataDirectoryPlaceholderInConnectionStringConfig_ReplacesDataDirectoryPlaceholder(
        string aDataDirectory,
        string aConnectionString,
        string aProviderName)
    {
        AppDomain.CurrentDomain.SetData("DataDirectory", aDataDirectory);
        var config = new Dictionary<string, string>
        {
            [$"ConnectionStrings:{UmbracoDbDsn}"] =
                $"{ConnectionStrings.DataDirectoryPlaceholder}/{aConnectionString}",
            [$"ConnectionStrings:{UmbracoDbDsn}_ProviderName"] = aProviderName,
        };

        var result = GetOptions(config);

        Assert.Multiple(() =>
        {
            Assert.That(result.Value.ConnectionString, Is.Not.Null);
            Assert.That(result.Value.ConnectionString, Contains.Substring($"{aDataDirectory}/{aConnectionString}"));

            Assert.That(result.Get(UmbracoDbDsn).ConnectionString, Is.Not.Null);
            Assert.That(result.Get(UmbracoDbDsn).ConnectionString, Contains.Substring($"{aDataDirectory}/{aConnectionString}"));
        });
    }
}
