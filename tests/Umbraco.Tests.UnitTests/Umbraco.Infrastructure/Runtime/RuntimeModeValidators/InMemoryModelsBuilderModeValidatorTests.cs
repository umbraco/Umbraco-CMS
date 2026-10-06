// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Runtime.RuntimeModeValidators;

[TestFixture]
public class InMemoryModelsBuilderModeValidatorTests
{
    private const string InMemoryAuto = "InMemoryAuto";

    [TestCase(RuntimeMode.BackofficeDevelopment, true)]
    [TestCase(RuntimeMode.BackofficeDevelopment, false)]
    [TestCase(RuntimeMode.Development, true)]
    [TestCase(RuntimeMode.Development, false)]
    [TestCase(RuntimeMode.Production, true)]
    [TestCase(RuntimeMode.Production, false)]
    public void Validate_WhenRuntimeGeneratedModeIsInForce_Fails(RuntimeMode runtimeMode, bool liveFactoryEnabled)
    {
        var sut = CreateSut(InMemoryAuto, liveFactoryEnabled);

        var result = sut.Validate(runtimeMode, out var validationErrorMessage);

        if (liveFactoryEnabled)
        {
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True);
                Assert.That(validationErrorMessage, Is.Null);
            });
        }
        else
        {
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.False);
                Assert.That(validationErrorMessage, Is.Not.Null.And.Not.Empty);
            });
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Validate_WhenModeIsLeftAtItsDefault_Succeeds(bool liveFactoryEnabled)
    {
        // The default is a mode that needs no runtime generation, and the package that can generate at runtime
        // removes this validator, so a site that configured nothing must never fail here.
        var sut = CreateSut(new ModelsBuilderSettings().ModelsMode, liveFactoryEnabled);

        var result = sut.Validate(RuntimeMode.BackofficeDevelopment, out var validationErrorMessage);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(validationErrorMessage, Is.Null);
        });
    }

    [TestCase(Constants.ModelsBuilder.ModelsModes.Nothing, true)]
    [TestCase(Constants.ModelsBuilder.ModelsModes.Nothing, false)]
    [TestCase(Constants.ModelsBuilder.ModelsModes.SourceCodeAuto, true)]
    [TestCase(Constants.ModelsBuilder.ModelsModes.SourceCodeAuto, false)]
    [TestCase(Constants.ModelsBuilder.ModelsModes.SourceCodeManual, true)]
    [TestCase(Constants.ModelsBuilder.ModelsModes.SourceCodeManual, false)]
    public void Validate_WhenModeDoesNotGenerateAtRuntime_Succeeds(string modelsMode, bool liveFactoryEnabled)
    {
        var sut = CreateSut(modelsMode, liveFactoryEnabled);

        var result = sut.Validate(RuntimeMode.BackofficeDevelopment, out var validationErrorMessage);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(validationErrorMessage, Is.Null);
        });
    }

    private static InMemoryModelsBuilderModeValidator CreateSut(string modelsMode, bool liveFactoryEnabled)
    {
        // Reading the mode in force, rather than the configured one, is what lets a mode set in code be
        // validated the same as one set in configuration.
        var settings = new ModelsBuilderSettings { ModelsMode = modelsMode };

        return new InMemoryModelsBuilderModeValidator(
            Mock.Of<IOptionsMonitor<ModelsBuilderSettings>>(m => m.CurrentValue == settings),
            Mock.Of<IAutoPublishedModelFactory>(m => m.Enabled == liveFactoryEnabled));
    }
}
