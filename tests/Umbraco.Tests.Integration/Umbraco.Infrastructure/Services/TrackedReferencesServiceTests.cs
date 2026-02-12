// Copyright (c) Umbraco.
// See LICENSE for more details.
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.Persistence.Relations;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal class TrackedReferencesServiceTests : UmbracoIntegrationTest
{
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private Content Root1 { get; set; }

    private Content Child1 { get; set; }

    private Content Root2 { get; set; }

    private IContentType ContentType { get; set; }

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);
        builder
            .AddNotificationHandler<ContentSavedNotification, ContentRelationsUpdate>();
    }

    [SetUp]
    public void Setup() => CreateTestData();

    protected virtual void CreateTestData()
    {
        ContentType = new ContentTypeBuilder()
            .WithName("Page")
            .AddPropertyType()
                .WithAlias("contentPicker")
                .WithName("ContentPicker")
                .WithDataTypeId(1046)
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.ContentPicker)
                .Done()
            .AddPropertyType()
                .WithAlias("contentPicker2")
                .WithName("ContentPicker2")
                .WithDataTypeId(1046)
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.ContentPicker)
                .Done()
            .Build();

        ContentTypeService.Save(ContentType);
        ContentType.AllowedContentTypes = [new ContentTypeSort(ContentType.Key, 0, ContentType.Alias)];
        ContentTypeService.Save(ContentType);

        Root1 = new ContentBuilder()
            .WithContentType(ContentType)
            .WithName("Root 1")
            .Build();

        ContentService.Save(Root1);
        ContentService.Publish(Root1, ["*"]);

        Child1 = new ContentBuilder()
            .WithContentType(ContentType)
            .WithName("Child 1")
            .WithParentId(Root1.Id)
            .Build();

        ContentService.Save(Child1);
        ContentService.Publish(Child1, ["*"]);

        Root2 = new ContentBuilder()
            .WithContentType(ContentType)
            .WithName("Root 2")
            .WithPropertyValues(new
            {
                contentPicker = Udi.Create(Constants.UdiEntityType.Document, Root1.Key),  // contentPicker is the alias of the property type
                contentPicker2 = Udi.Create(Constants.UdiEntityType.Document, Child1.Key),
            })
            .Build();

        ContentService.Save(Root2);
        ContentService.Publish(Root2, ["*"]);
    }

    [Test]
    public async Task Get_Pages_That_Reference_This()
    {
        var sut = GetRequiredService<ITrackedReferencesService>();

        var actual = await sut.GetPagedRelationsForItemAsync(Root1.Key, UmbracoObjectTypes.Document, 0, 10, true);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(actual.Success);
            Assert.AreEqual(1, actual.Result.Total);
            var item = actual.Result.Items.FirstOrDefault();
            Assert.AreEqual(Root2.ContentType.Alias, item?.ContentTypeAlias);
            Assert.AreEqual(Root2.Key, item?.NodeKey);
        });
    }

    [Test]
    public async Task Get_Relations_For_Non_Existing_Page_Returns_Not_Found()
    {
        var sut = GetRequiredService<ITrackedReferencesService>();

        var actual = await sut.GetPagedRelationsForItemAsync(Guid.NewGuid(), UmbracoObjectTypes.Document, 0, 10, true);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(actual.Success);
            Assert.AreEqual(GetReferencesOperationStatus.ContentNotFound, actual.Status);
        });
    }

    [Test]
    public async Task Get_Descendants_In_References_For_Existing_Page_Returns_Expected_Results()
    {
        var sut = GetRequiredService<ITrackedReferencesService>();

        var actual = await sut.GetPagedDescendantsInReferencesAsync(Root1.Key, UmbracoObjectTypes.Document, 0, 10, true);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(actual.Success);
            Assert.AreEqual(GetReferencesOperationStatus.Success, actual.Status);

            var itemKeys = actual.Result.Items.Select(x => x.NodeKey).ToList();
            Assert.IsFalse(itemKeys.Contains(Root1.Key)); // Should not return the parent itself (see: https://github.com/umbraco/Umbraco-CMS/pull/21162)
            Assert.AreEqual(1, itemKeys.Count);
            Assert.IsTrue(itemKeys.Contains(Child1.Key));
        });
    }

    [Test]
    public async Task Get_Descendants_In_References_For_Non_Existing_Page_Returns_Not_Found()
    {
        var sut = GetRequiredService<ITrackedReferencesService>();

        var actual = await sut.GetPagedDescendantsInReferencesAsync(Guid.NewGuid(), UmbracoObjectTypes.Document, 0, 10, true);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(actual.Success);
            Assert.AreEqual(GetReferencesOperationStatus.ContentNotFound, actual.Status);
        });
    }

    [Test]
    public async Task Does_Not_Return_References_If_Item_Is_Not_Referenced()
    {
        var sut = GetRequiredService<ITrackedReferencesService>();

        var actual = await sut.GetPagedRelationsForItemAsync(Root2.Key, UmbracoObjectTypes.Document, 0, 10, true);

        Assert.IsTrue(actual.Success);
        Assert.AreEqual(0, actual.Result.Total);
    }

    [Test]
    public async Task Get_Pages_That_Reference_Recycle_Bin_Contents()
    {
        ContentService.MoveToRecycleBin(Root1);

        var sut = GetRequiredService<ITrackedReferencesService>();

        var actual = await sut.GetPagedRelationsForRecycleBinAsync(UmbracoObjectTypes.Document, 0, 10, true);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, actual.Total);
            var item = actual.Items.FirstOrDefault();
            Assert.AreEqual(Root2.ContentType.Alias, item?.ContentTypeAlias);
            Assert.AreEqual(Root2.Key, item?.NodeKey);
        });
    }
}
