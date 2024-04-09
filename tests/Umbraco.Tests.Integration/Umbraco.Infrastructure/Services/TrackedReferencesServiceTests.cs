// Copyright (c) Umbraco.
// See LICENSE for more details.
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class TrackedReferencesServiceTests : UmbracoIntegrationTest
{

    protected IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    protected IContentService ContentService => GetRequiredService<IContentService>();
    protected IRelationService RelationService => GetRequiredService<IRelationService>();


    protected Content Root1 { get; private set; }
    protected Content Root2 { get; private set; }

    protected IContentType ContentType { get; private set; }

    [SetUp]
    public void Setup() => CreateTestData();

    public virtual void CreateTestData()
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
                contentPicker = Udi.Create(Constants.UdiEntityType.Document , Root1.Key) // contentPicker is the alias of the property type
            })
            .Build();

        ContentService.Save(Root2);
        ContentService.Publish(Root2, ["*"]);

        var x = RelationService.GetAllRelations();
    }

    [Test]
    public async Task Get_Pages_That_References_This()
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
    public async Task Do_not_return_references_if_items_is_not_referenced()
    {
        var sut = GetRequiredService<ITrackedReferencesService>();

        var actual = await sut.GetPagedRelationsForItemAsync(Root2.Key, 0, 10, true);

        Assert.AreEqual(0, actual.Total);
    }

}
