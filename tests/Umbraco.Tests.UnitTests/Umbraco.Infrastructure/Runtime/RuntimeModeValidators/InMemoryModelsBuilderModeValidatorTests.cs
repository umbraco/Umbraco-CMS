// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Runtime.RuntimeModeValidators;

[TestFixture]
public class InMemoryModelsBuilderModeValidatorTests
{
    private const string InMemoryAuto = "InMemoryAuto";

    [TestCase(RuntimeMode.BackofficeDevelopment)]
    [TestCase(RuntimeMode.Development)]
    [TestCase(RuntimeMode.Production)]
    public void Cannot_Validate_An_Explicitly_Configured_Runtime_Generated_Mode(RuntimeMode runtimeMode)
    {
        var sut = CreateSut(InMemoryAuto);

        var result = sut.Validate(runtimeMode, out var validationErrorMessage);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            Assert.That(validationErrorMessage, Is.Not.Null.And.Not.Empty);
        });
    }

    [Test]
    public void Can_Validate_When_No_Mode_Has_Been_Configured()
    {
        // The default is a mode that needs no runtime generation, and the package that can generate at runtime
        // removes this validator, so an unconfigured site must never fail here.
        var sut = CreateSut(configuredModelsMode: null);

        var result = sut.Validate(RuntimeMode.BackofficeDevelopment, out var validationErrorMessage);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(validationErrorMessage, Is.Null);
        });
    }

    [TestCase(Constants.ModelsBuilder.ModelsModes.Nothing)]
    [TestCase(Constants.ModelsBuilder.ModelsModes.SourceCodeAuto)]
    [TestCase(Constants.ModelsBuilder.ModelsModes.SourceCodeManual)]
    public void Can_Validate_A_Mode_That_Does_Not_Generate_At_Runtime(string configuredModelsMode)
    {
        var sut = CreateSut(configuredModelsMode);

        var result = sut.Validate(RuntimeMode.BackofficeDevelopment, out var validationErrorMessage);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(validationErrorMessage, Is.Null);
        });
    }

    private static InMemoryModelsBuilderModeValidator CreateSut(string? configuredModelsMode)
    {
        var configurationValues = new Dictionary<string, string?>();
        if (configuredModelsMode is not null)
        {
            configurationValues[Constants.Configuration.ConfigModelsMode] = configuredModelsMode;
        }

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationValues)
            .Build();

        return new InMemoryModelsBuilderModeValidator(configuration);
    }
}
