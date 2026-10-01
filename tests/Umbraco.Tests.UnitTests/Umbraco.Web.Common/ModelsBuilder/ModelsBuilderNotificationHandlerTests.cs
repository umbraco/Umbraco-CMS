// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.ModelsBuilder;
using Umbraco.Cms.Web.Common.ModelsBuilder;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.ModelsBuilder;

[TestFixture]
public class ModelsBuilderNotificationHandlerTests
{
    private const string TypedMarkup = "@inherits UmbracoViewPage<TModel>";

    [TestCase(Constants.ModelsBuilder.InMemoryAutoModelsMode)]
    [TestCase(Constants.ModelsBuilder.ModelsModes.SourceCodeAuto)]
    [TestCase(Constants.ModelsBuilder.ModelsModes.SourceCodeManual)]
    public void Can_Generate_Typed_Template_When_Models_Are_Available(string modelsMode)
    {
        Template template = CreateNewTemplate();

        CreateSut(modelsMode, liveFactoryEnabled: true).Handle(CreateNotification(template));

        Assert.AreEqual(TypedMarkup, template.Content);
    }

    [TestCase(Constants.ModelsBuilder.ModelsModes.SourceCodeAuto)]
    [TestCase(Constants.ModelsBuilder.ModelsModes.SourceCodeManual)]
    public void Can_Generate_Typed_Template_Without_A_Live_Factory_When_Models_Are_Generated_As_Source_Code(string modelsMode)
    {
        Template template = CreateNewTemplate();

        CreateSut(modelsMode, liveFactoryEnabled: false).Handle(CreateNotification(template));

        Assert.AreEqual(TypedMarkup, template.Content);
    }

    [Test]
    public void Cannot_Generate_Typed_Template_When_Models_Are_Only_Generated_At_Runtime_And_No_Live_Factory_Is_Available()
    {
        Template template = CreateNewTemplate();

        CreateSut(Constants.ModelsBuilder.InMemoryAutoModelsMode, liveFactoryEnabled: false).Handle(CreateNotification(template));

        Assert.That(template.Content, Is.Null.Or.Empty);
    }

    [Test]
    public void Cannot_Generate_Typed_Template_When_No_Models_Are_Generated()
    {
        Template template = CreateNewTemplate();

        CreateSut(Constants.ModelsBuilder.ModelsModes.Nothing, liveFactoryEnabled: true).Handle(CreateNotification(template));

        Assert.That(template.Content, Is.Null.Or.Empty);
    }

    private static ModelsBuilderNotificationHandler CreateSut(string modelsMode, bool liveFactoryEnabled)
    {
        var settings = new ModelsBuilderSettings { ModelsMode = modelsMode };

        var defaultViewContentProvider = new Mock<IDefaultViewContentProvider>();
        defaultViewContentProvider
            .Setup(x => x.GetDefaultFileContent(
                It.IsAny<string?>(),
                It.IsNotNull<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>()))
            .Returns(TypedMarkup);

        return new ModelsBuilderNotificationHandler(
            new OptionsWrapper<ModelsBuilderSettings>(settings),
            ShortStringHelper,
            Mock.Of<IModelsBuilderDashboardProvider>(),
            defaultViewContentProvider.Object,
            CreatePublishedModelFactory(liveFactoryEnabled));
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

    private static IShortStringHelper ShortStringHelper { get; } =
        new DefaultShortStringHelper(new DefaultShortStringHelperConfig());

    private static Template CreateNewTemplate() => new(ShortStringHelper, "TestType", "testType");

    private static TemplateSavingNotification CreateNotification(ITemplate template) => new(
        template,
        new EventMessages(),
        createTemplateForContentType: true,
        contentTypeAlias: "testType");
}
