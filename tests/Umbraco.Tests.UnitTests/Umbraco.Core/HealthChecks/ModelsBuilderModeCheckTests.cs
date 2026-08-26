// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.HealthChecks;
using Umbraco.Cms.Core.HealthChecks.Checks.Configuration;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.HealthChecks;

[TestFixture]
public class ModelsBuilderModeCheckTests
{
    [Test]
    public async Task When_Explicitly_Configured_Mode_Cannot_Be_Satisfied_Returns_Error()
    {
        HealthCheckStatus status = await GetStatus(Constants.ModelsBuilder.InMemoryAutoModelsMode, liveFactoryEnabled: false, modelsModeConfigured: true);

        Assert.Multiple(() =>
        {
            Assert.That(status.ResultType, Is.EqualTo(StatusResultType.Error));
            Assert.That(status.Message, Is.EqualTo("modelsBuilderModeCheckConfiguredErrorMessage"));
        });
    }

    [Test]
    public async Task When_Defaulted_Mode_Cannot_Be_Satisfied_Returns_Warning()
    {
        HealthCheckStatus status = await GetStatus(Constants.ModelsBuilder.InMemoryAutoModelsMode, liveFactoryEnabled: false, modelsModeConfigured: false);

        Assert.Multiple(() =>
        {
            Assert.That(status.ResultType, Is.EqualTo(StatusResultType.Warning));
            Assert.That(status.Message, Is.EqualTo("modelsBuilderModeCheckDefaultErrorMessage"));
        });
    }

    [Test]
    public async Task When_Live_Factory_Is_Available_Returns_Success()
    {
        HealthCheckStatus status = await GetStatus(Constants.ModelsBuilder.InMemoryAutoModelsMode, liveFactoryEnabled: true, modelsModeConfigured: true);

        Assert.Multiple(() =>
        {
            Assert.That(status.ResultType, Is.EqualTo(StatusResultType.Success));
            Assert.That(status.Message, Is.EqualTo("modelsBuilderModeCheckSuccessMessage"));
        });
    }

    [TestCase(Constants.ModelsBuilder.ModelsModes.Nothing)]
    [TestCase(Constants.ModelsBuilder.ModelsModes.SourceCodeAuto)]
    [TestCase(Constants.ModelsBuilder.ModelsModes.SourceCodeManual)]
    public async Task When_Mode_Does_Not_Require_A_Live_Factory_Returns_Success(string modelsMode)
    {
        HealthCheckStatus status = await GetStatus(modelsMode, liveFactoryEnabled: false, modelsModeConfigured: true);

        Assert.That(status.ResultType, Is.EqualTo(StatusResultType.Success));
    }

    private static async Task<HealthCheckStatus> GetStatus(string modelsMode, bool liveFactoryEnabled, bool modelsModeConfigured)
    {
        var settings = new ModelsBuilderSettings { ModelsMode = modelsMode };

        var configurationValues = new Dictionary<string, string?>();
        if (modelsModeConfigured)
        {
            configurationValues[Constants.Configuration.ConfigModelsMode] = modelsMode;
        }

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationValues)
            .Build();

        var check = new ModelsBuilderModeCheck(
            MockTextService(),
            Mock.Of<IOptionsMonitor<ModelsBuilderSettings>>(m => m.CurrentValue == settings),
            CreatePublishedModelFactory(liveFactoryEnabled),
            configuration);

        IEnumerable<HealthCheckStatus> statuses = await check.GetStatusAsync();
        return statuses.Single();
    }

    private static IPublishedModelFactory CreatePublishedModelFactory(bool liveFactoryEnabled)
    {
        if (liveFactoryEnabled is false)
        {
            return Mock.Of<IPublishedModelFactory>();
        }

        var factory = new Mock<IAutoPublishedModelFactory>();
        factory.SetupGet(x => x.Enabled).Returns(true);
        return factory.Object;
    }

    // Returns the alias so tests can assert on which key was used.
    private static ILocalizedTextService MockTextService()
    {
        var mock = new Mock<ILocalizedTextService>();
        mock.Setup(x => x.Localize(
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CultureInfo?>(),
                It.IsAny<IDictionary<string, string?>>()))
            .Returns((string? _, string? alias, CultureInfo? _, IDictionary<string, string?> _) => alias ?? string.Empty);
        return mock.Object;
    }
}
