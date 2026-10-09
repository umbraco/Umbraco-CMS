using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Configuration;

[TestFixture]
public class CoreDebugSettingsConfigurationTests
{
    private static CoreDebugSettings GetSettings(IDictionary<string, string> configValues)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        var services = new ServiceCollection();
        var builder = new UmbracoBuilder(services, configuration, TestHelper.GetMockedTypeLoader());
        builder.AddConfiguration();

        return services.BuildServiceProvider().GetRequiredService<IOptions<CoreDebugSettings>>().Value;
    }

    [Test]
    public void Can_Bind_From_Debug_Section()
    {
        CoreDebugSettings settings = GetSettings(new Dictionary<string, string>
        {
            ["Umbraco:CMS:Debug:LogIncompletedScopes"] = "true",
        });

        Assert.That(settings.LogIncompletedScopes, Is.True);
    }

    // TODO (V19): remove this test when the legacy "Umbraco:CMS:Core:Debug" section bind is dropped.
    [Test]
    public void Can_Bind_From_Legacy_Core_Debug_Section()
    {
        CoreDebugSettings settings = GetSettings(new Dictionary<string, string>
        {
            ["Umbraco:CMS:Core:Debug:LogIncompletedScopes"] = "true",
        });

        Assert.That(settings.LogIncompletedScopes, Is.True);
    }
}
