using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Models.Searching;
using Umbraco.Cms.Search.Core.Models.Searching.Filtering;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Tests.Search.Examine.Integration.Tests.ContentTests.SearchService;

public class InvariantDocumentTests : SearcherTestBase
{
    [TestCase(true)]
    [TestCase(false)]
    public async Task SearchWithNoParamsYieldsNoDocuments(bool publish)
    {
        var indexAlias = GetIndexAlias(publish);

        SearchResult results = await Searcher.SearchAsync(indexAlias, null, null, null, null, null, null, null, 0, 100);
        Assert.That(results.Total, Is.EqualTo(0));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanSearchName(bool publish)
    {
        var indexAlias = GetIndexAlias(publish);

        SearchResult results = await Searcher.SearchAsync(indexAlias, "Test", null, null, null, null, null, null, 0, 100);
        Assert.That(results.Total, Is.EqualTo(1));
        Assert.That(results.Documents.First().Id, Is.EqualTo(RootKey));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanNotSearchDeletedName(bool publish)
    {
        await WaitForIndexing(GetIndexAlias(true), () =>
        {
            IContent? content = ContentService.GetById(RootKey);
            ContentService.Delete(content!);
            return Task.CompletedTask;
        });

        var indexAlias = GetIndexAlias(publish);

        SearchResult results = await Searcher.SearchAsync(indexAlias, "Test", null, null, null, null, null, null, 0, 100);
        Assert.That(results.Total, Is.EqualTo(0));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanSearchTextProperty(bool publish)
    {
        var indexAlias = GetIndexAlias(publish);

        SearchResult results = await Searcher.SearchAsync(indexAlias, "root title", null, null, null, null, null, null, 0, 100);
        Assert.That(results.Total, Is.EqualTo(1));
        Assert.That(results.Documents.First().Id, Is.EqualTo(RootKey));
    }

    [TestCase("title", "updated title", false)]
    [TestCase("title", "updated title", true)]
    [TestCase("title", "something-with-dashes", false)]
    [TestCase("title", "something-with-dashes", true)]
    [TestCase("title", "something\"with\"quotes", false)]
    [TestCase("title", "something\"with\"quotes", true)]
    public async Task CanSearchUpdatedProperties(string propertyName, object updatedValue, bool publish)
    {
        await UpdateProperty(propertyName, updatedValue, publish);

        var indexAlias = GetIndexAlias(publish);

        SearchResult results = await Searcher.SearchAsync(indexAlias, updatedValue.ToString(), null, null, null, null, null, null, 0, 100);
        Assert.That(results.Total, Is.EqualTo(1));
        Assert.That(results.Documents.First().Id, Is.EqualTo(RootKey));
    }

    [TestCase("title", "updated title", false)]
    [TestCase("title", "updated title", true)]
    [TestCase("title", "something-with-dashes", false)]
    [TestCase("title", "something-with-dashes", true)]
    [TestCase("title", "something\"with\"quotes", false)]
    [TestCase("title", "something\"with\"quotes", true)]
    public async Task CanFilterByUpdatedProperties(string propertyName, string updatedValue, bool publish)
    {
        await UpdateProperty(propertyName, updatedValue, publish);

        var indexAlias = GetIndexAlias(publish);

        SearchResult results = await Searcher.SearchAsync(
            indexAlias: indexAlias,
            filters: new List<Filter> { new TextFilter("title", [updatedValue], false) });

        Assert.That(results.Total, Is.EqualTo(1));
        Assert.That(results.Documents.First().Id, Is.EqualTo(RootKey));
    }

    [Test]
    public async Task SearchCanCrossTextualRelevanceBoundaries()
    {
        var indexAlias = GetIndexAlias(true);

        // "test" is from the document name (TextsR1), "title" is from the document property (Texts)
        SearchResult results = await Searcher.SearchAsync(
            indexAlias: indexAlias,
            query: "test title");

        Assert.That(results.Total, Is.EqualTo(1));
        Assert.That(results.Documents.First().Id, Is.EqualTo(RootKey));
    }

    [SetUp]
    public async Task CreateInvariantDocument()
    {
        DataType dataType = new DataTypeBuilder()
            .WithId(0)
            .WithoutIdentity()
            .WithDatabaseType(ValueStorageType.Decimal)
            .AddEditor()
            .WithAlias(Constants.PropertyEditors.Aliases.Decimal)
            .Done()
            .Build();

        await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        IContentType contentType = new ContentTypeBuilder()
            .WithAlias("invariant")
            .AddPropertyType()
            .WithAlias("title")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .AddPropertyType()
            .WithAlias("count")
            .WithDataTypeId(-51)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Integer)
            .Done()
            .AddPropertyType()
            .WithAlias("datetime")
            .WithDataTypeId(Constants.DataTypes.DateTime)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.DateTime)
            .Done()
            .AddPropertyType()
            .WithAlias("decimalproperty")
            .WithDataTypeId(dataType.Id)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Decimal)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        Content root = new ContentBuilder()
            .WithKey(RootKey)
            .WithContentType(contentType)
            .WithName("Test")
            .WithPropertyValues(
                new
                {
                    title = "The root title",
                    count = 12,
                    datetime = CurrentDateTimeOffset.DateTime,
                    decimalproperty = DecimalValue
                })
            .Build();

        await WaitForIndexing(GetIndexAlias(true), () =>
        {
            ContentService.Save(root);
            ContentService.Publish(root, new[] {"*"});
            return Task.CompletedTask;
        });

        IContent? content = ContentService.GetById(RootKey);
        Assert.That(content, Is.Not.Null);
    }

    private async Task UpdateProperty(string propertyName, object value, bool publish)
    {
        IContent content = ContentService.GetById(RootKey)!;
        content.SetValue(propertyName, value);

        await WaitForIndexing(GetIndexAlias(publish), () =>
        {
            if (publish)
            {
                ContentService.Save(content);
                ContentService.Publish(content, ["*"]);
            }
            else
            {
                ContentService.Save(content);
            }
            return Task.CompletedTask;
        });
    }
}
