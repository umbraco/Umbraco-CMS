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

    public enum ModelFactory
    {
        /// <summary>
        /// A factory that cannot generate at runtime at all, as supplied when nothing provides a live one.
        /// </summary>
        NotLive,

        /// <summary>
        /// A factory able to generate at runtime, but not currently doing so.
        /// </summary>
        LiveButDisabled,

        /// <summary>
        /// A factory generating at runtime, whichever component supplied it.
        /// </summary>
        Live,
    }

    [TestCase(RuntimeMode.BackofficeDevelopment, ModelFactory.NotLive)]
    [TestCase(RuntimeMode.BackofficeDevelopment, ModelFactory.LiveButDisabled)]
    [TestCase(RuntimeMode.Development, ModelFactory.NotLive)]
    [TestCase(RuntimeMode.Development, ModelFactory.LiveButDisabled)]
    [TestCase(RuntimeMode.Production, ModelFactory.NotLive)]
    [TestCase(RuntimeMode.Production, ModelFactory.LiveButDisabled)]
    public void Validate_WhenRuntimeGeneratedModeIsInForceWithoutALiveFactory_Fails(RuntimeMode runtimeMode, ModelFactory modelFactory)
    {
        var sut = CreateSut(InMemoryAuto, modelFactory);

        var result = sut.Validate(runtimeMode, out var validationErrorMessage);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            Assert.That(validationErrorMessage, Is.Not.Null.And.Not.Empty);
        });
    }

    [TestCase(RuntimeMode.BackofficeDevelopment)]
    [TestCase(RuntimeMode.Development)]
    [TestCase(RuntimeMode.Production)]
    public void Validate_WhenRuntimeGeneratedModeIsInForceWithALiveFactory_Succeeds(RuntimeMode runtimeMode)
    {
        // The mode is satisfied by whatever supplies the factory, so the validator has nothing to object to.
        var sut = CreateSut(InMemoryAuto, ModelFactory.Live);

        var result = sut.Validate(runtimeMode, out var validationErrorMessage);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(validationErrorMessage, Is.Null);
        });
    }

    [TestCase(ModelFactory.NotLive)]
    [TestCase(ModelFactory.LiveButDisabled)]
    [TestCase(ModelFactory.Live)]
    public void Validate_WhenModeIsLeftAtItsDefault_Succeeds(ModelFactory modelFactory)
    {
        // The default is a mode that needs no runtime generation, so a site that configured nothing must never
        // fail here, whatever factory is in force.
        var sut = CreateSut(new ModelsBuilderSettings().ModelsMode, modelFactory);

        var result = sut.Validate(RuntimeMode.BackofficeDevelopment, out var validationErrorMessage);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(validationErrorMessage, Is.Null);
        });
    }

    [TestCase(Constants.ModelsBuilder.ModelsModes.Nothing, ModelFactory.NotLive)]
    [TestCase(Constants.ModelsBuilder.ModelsModes.Nothing, ModelFactory.Live)]
    [TestCase(Constants.ModelsBuilder.ModelsModes.SourceCodeAuto, ModelFactory.NotLive)]
    [TestCase(Constants.ModelsBuilder.ModelsModes.SourceCodeAuto, ModelFactory.Live)]
    [TestCase(Constants.ModelsBuilder.ModelsModes.SourceCodeManual, ModelFactory.NotLive)]
    [TestCase(Constants.ModelsBuilder.ModelsModes.SourceCodeManual, ModelFactory.Live)]
    public void Validate_WhenModeDoesNotGenerateAtRuntime_Succeeds(string modelsMode, ModelFactory modelFactory)
    {
        var sut = CreateSut(modelsMode, modelFactory);

        var result = sut.Validate(RuntimeMode.BackofficeDevelopment, out var validationErrorMessage);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(validationErrorMessage, Is.Null);
        });
    }

    [Test]
    public void Validate_WhenModeDoesNotGenerateAtRuntime_DoesNotBuildTheModelFactory()
    {
        var built = false;
        var sut = CreateSut(
            Constants.ModelsBuilder.ModelsModes.Nothing,
            new Lazy<IPublishedModelFactory>(() =>
            {
                built = true;
                return Mock.Of<IPublishedModelFactory>();
            }));

        sut.Validate(RuntimeMode.BackofficeDevelopment, out _);

        // Validation runs while the runtime level is being determined, so a mode that cannot need a factory
        // must not be what causes one to be built that early in the boot.
        Assert.That(built, Is.False);
    }

    private static InMemoryModelsBuilderModeValidator CreateSut(string modelsMode, ModelFactory modelFactory)
        => CreateSut(modelsMode, new Lazy<IPublishedModelFactory>(() => modelFactory switch
        {
            ModelFactory.NotLive => Mock.Of<IPublishedModelFactory>(),
            ModelFactory.LiveButDisabled => Mock.Of<IAutoPublishedModelFactory>(m => m.Enabled == false),
            ModelFactory.Live => Mock.Of<IAutoPublishedModelFactory>(m => m.Enabled == true),
            _ => throw new ArgumentOutOfRangeException(nameof(modelFactory)),
        }));

    private static InMemoryModelsBuilderModeValidator CreateSut(string modelsMode, Lazy<IPublishedModelFactory> publishedModelFactory)
    {
        // Reading the mode in force, rather than the configured one, is what lets a mode set in code be
        // validated the same as one set in configuration.
        var settings = new ModelsBuilderSettings { ModelsMode = modelsMode };

        return new InMemoryModelsBuilderModeValidator(
            Mock.Of<IOptionsMonitor<ModelsBuilderSettings>>(m => m.CurrentValue == settings),
            publishedModelFactory);
    }
}
