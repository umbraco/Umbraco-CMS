// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using Moq;
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
    public void Validate_WhenRuntimeGeneratedModeIsInForce_Fails(RuntimeMode runtimeMode)
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
    public void Validate_WhenModeIsLeftAtItsDefault_Succeeds()
    {
        // The default is a mode that needs no runtime generation, and the package that can generate at runtime
        // removes this validator, so a site that configured nothing must never fail here.
        var sut = CreateSut(new ModelsBuilderSettings().ModelsMode);

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
    public void Validate_WhenModeDoesNotGenerateAtRuntime_Succeeds(string modelsMode)
    {
        var sut = CreateSut(modelsMode);

        var result = sut.Validate(RuntimeMode.BackofficeDevelopment, out var validationErrorMessage);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(validationErrorMessage, Is.Null);
        });
    }

    private static InMemoryModelsBuilderModeValidator CreateSut(string modelsMode)
    {
        // Reading the mode in force, rather than the configured one, is what lets a mode set in code be
        // validated the same as one set in configuration.
        var settings = new ModelsBuilderSettings { ModelsMode = modelsMode };

        return new InMemoryModelsBuilderModeValidator(
            Mock.Of<IOptionsMonitor<ModelsBuilderSettings>>(m => m.CurrentValue == settings));
    }
}
