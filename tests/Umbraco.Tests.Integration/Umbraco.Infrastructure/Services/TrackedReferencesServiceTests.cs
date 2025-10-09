// Copyright (c) Umbraco.
// See LICENSE for more details.
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
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
                .WithAlias("ContentPicker")
                .WithName("contentPicker")
                .WithDataTypeId(1046)
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.ContentPicker)
            .Done()
            .Build();

        ContentTypeService.Save(ContentType);

        Root1 = new ContentBuilder()
            .WithContentType(ContentType)
            .WithName("Root 1")
            .Build();

        ContentService.Save(Root1);
        ContentService.Publish(Root1, ["*"]);

        Root2 = new ContentBuilder()
            .WithContentType(ContentType)
            .WithName("Root 2")
            .WithPropertyValues(new
            {
                contentPicker = Udi.Create(Constants.UdiEntityType.Document, Root1.Key) // contentPicker is the alias of the property type
            })
            .Build();

        ContentService.Save(Root2);
        ContentService.Publish(Root2, ["*"]);
    }

    [Test]
    public async Task Get_Pages_That_Reference_This()
    {
        var sut = GetRequiredService<ITrackedReferencesService>();

        var actual = await sut.GetPagedRelationsForItemAsync(Root1.Key, 0, 10, true);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, actual.Total);
            var item = actual.Items.FirstOrDefault();
            Assert.AreEqual(Root2.ContentType.Alias, item?.ContentTypeAlias);
            Assert.AreEqual(Root2.Key, item?.NodeKey);
        });
    }

    [Test]
    public async Task Does_Not_Return_References_If_Item_Is_Not_Referenced()
    {
        var sut = GetRequiredService<ITrackedReferencesService>();

        var actual = await sut.GetPagedRelationsForItemAsync(Root2.Key, 0, 10, true);

        Assert.AreEqual(0, actual.Total);
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
