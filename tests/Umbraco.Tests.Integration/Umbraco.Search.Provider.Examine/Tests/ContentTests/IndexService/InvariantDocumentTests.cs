using Examine;
using Examine.Search;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Provider.Examine.Helpers;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Constants = Umbraco.Cms.Search.Provider.Examine.Constants;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Tests.ContentTests.IndexService;

public class InvariantDocumentTests : IndexTestBase
{
    [TestCase(true)]
    [TestCase(false)]
    public void CanIndexAnyDocument(bool publish)
    {
        IIndex index = GetIndex(GetIndexAlias(publish));

        ISearchResult[] results = index.Searcher.CreateQuery().All().Execute().ToArray();
        Assert.That(results.Length, Is.EqualTo(1));
        Assert.That(results[0].Id, Is.EqualTo(RootKey.ToString()));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanRemoveAnyDocument(bool publish)
    {
        var indexAlias = GetIndexAlias(publish);
        await WaitForIndexing(indexAlias, () =>
        {
            IContent content = ContentService.GetById(RootKey)!;
            ContentService.Delete(content);
            return Task.CompletedTask;
        });

        IIndex index = GetIndex(indexAlias);
        ISearchResults results = index.Searcher.CreateQuery().All().Execute();
        Assert.That(results, Is.Empty);
    }

    [Test]
    public async Task CanRemoveUnpublishedDocument()
    {
        await WaitForIndexing(Cms.Core.Constants.IndexAliases.PublishedContent, () =>
        {
            IContent content = ContentService.GetById(RootKey)!;
            ContentService.Unpublish(content);
            return Task.CompletedTask;
        });


        IIndex index = GetIndex(Cms.Core.Constants.IndexAliases.PublishedContent);
        ISearchResults results = index.Searcher.CreateQuery().All().Execute();
        Assert.That(results, Is.Empty);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void CanIndexTextProperty(bool publish)
    {
        IIndex index = GetIndex(GetIndexAlias(publish));

        IOrdering queryBuilder = index.Searcher.CreateQuery().All();
        var fieldName = FieldNameHelper.FieldName("title", Constants.FieldValues.Texts);
        queryBuilder.SelectField(fieldName);
        ISearchResults results = queryBuilder.Execute();
        Assert.That(results, Is.Not.Empty);
        Assert.That(results.First().Values.First(x => x.Key == fieldName).Value, Is.EqualTo("The root title"));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void CanIndexIntegerValues(bool publish)
    {
        IIndex index = GetIndex(GetIndexAlias(publish));

        IOrdering queryBuilder = index.Searcher.CreateQuery().All();
        var fieldName = FieldNameHelper.FieldName("count", Constants.FieldValues.Integers);
        queryBuilder.SelectField(fieldName);
        ISearchResults results = queryBuilder.Execute();
        Assert.That(results, Is.Not.Empty);
        Assert.That(results.First().Values.First(x => x.Key == fieldName).Value, Is.EqualTo("12"));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void CanIndexDecimalValues(bool publish)
    {
        IIndex index = GetIndex(GetIndexAlias(publish));

        IOrdering queryBuilder = index.Searcher.CreateQuery().All();
        var fieldName = FieldNameHelper.FieldName("decimalproperty", Constants.FieldValues.Decimals);
        queryBuilder.SelectField(fieldName);
        ISearchResults results = queryBuilder.Execute();
        Assert.That(results, Is.Not.Empty);
        Assert.That(
            double.Parse(results.First().Values.First(x => x.Key == fieldName).Value),
            Is.EqualTo((double)DecimalValue));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void CanIndexDateTimeValues(bool publish)
    {
        IIndex index = GetIndex(GetIndexAlias(publish));

        IOrdering queryBuilder = index.Searcher.CreateQuery().All();
        var fieldName = FieldNameHelper.FieldName("datetime", Constants.FieldValues.DateTimeOffsets);
        queryBuilder.SelectField(fieldName);
        ISearchResults results = queryBuilder.Execute();
        Assert.That(results, Is.Not.Empty);
        Assert.That(results.First().Values.First().Value, Is.EqualTo(CurrentDateTime.Ticks.ToString()));
    }


    [TestCase("title", "updated title", false)]
    [TestCase("title", "updated title", true)]
    [TestCase("count", 12, false)]
    [TestCase("count", 12, true)]
    [TestCase("decimalproperty", 1.45, false)]
    [TestCase("decimalproperty", 1.45, true)]
    public async Task CanIndexUpdatedProperties(string propertyName, object updatedValue, bool publish)
    {
        await UpdateProperty(propertyName, updatedValue, publish);

        IIndex index = GetIndex(GetIndexAlias(publish));

        ISearchResults results = index.Searcher.Search(updatedValue.ToString()!);
        Assert.That(results, Is.Not.Empty);
        Assert.That(results.First().Values.First(x => x.Value == updatedValue.ToString()).Value, Is.EqualTo(updatedValue.ToString()));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void CanIndexAggregatedTexts(bool publish)
    {
        IIndex index = GetIndex(GetIndexAlias(publish));

        IOrdering queryBuilder = index.Searcher.CreateQuery().All();
        queryBuilder.SelectField(Constants.SystemFields.AggregatedTexts);
        ISearchResults results = queryBuilder.Execute();
        Assert.That(results, Is.Not.Empty);
        Assert.That(results.First().AllValues.First(x => x.Key == Constants.SystemFields.AggregatedTexts).Value.Contains("The root title"), Is.True);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanGetDocumentCount(bool publish)
    {
        var indexAlias = GetIndexAlias(publish);
        var count = (await Indexer.GetMetadataAsync(indexAlias)).DocumentCount;
        Assert.That(count, Is.EqualTo(1));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanGetDocumentCount_AfterDeletion(bool publish)
    {
        var indexAlias = GetIndexAlias(publish);
        await WaitForIndexing(indexAlias, () =>
        {
            IContent content = ContentService.GetById(RootKey)!;
            ContentService.Delete(content);
            return Task.CompletedTask;
        });

        var count = (await Indexer.GetMetadataAsync(indexAlias)).DocumentCount;
        Assert.That(count, Is.EqualTo(0));
    }


    [SetUp]
    public async Task CreateInvariantDocument()
    {
        DataType dataType = new DataTypeBuilder()
            .WithId(0)
            .WithoutIdentity()
            .WithDatabaseType(ValueStorageType.Decimal)
            .AddEditor()
            .WithAlias(Cms.Core.Constants.PropertyEditors.Aliases.Decimal)
            .Done()
            .Build();

        await DataTypeService.CreateAsync(dataType, Cms.Core.Constants.Security.SuperUserKey);
        IContentType contentType = new ContentTypeBuilder()
            .WithAlias("invariant")
            .AddPropertyType()
            .WithAlias("title")
            .WithDataTypeId(Cms.Core.Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Cms.Core.Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .AddPropertyType()
            .WithAlias("count")
            .WithDataTypeId(-51)
            .WithPropertyEditorAlias(Cms.Core.Constants.PropertyEditors.Aliases.Integer)
            .Done()
            .AddPropertyType()
            .WithAlias("datetime")
            .WithDataTypeId(Cms.Core.Constants.DataTypes.DateTime)
            .WithPropertyEditorAlias(Cms.Core.Constants.PropertyEditors.Aliases.DateTime)
            .Done()
            .AddPropertyType()
            .WithAlias("decimalproperty")
            .WithDataTypeId(dataType.Id)
            .WithPropertyEditorAlias(Cms.Core.Constants.PropertyEditors.Aliases.Decimal)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(contentType, Cms.Core.Constants.Security.SuperUserKey);

        CurrentDateTime = CurrentDateTimeOffset.DateTime.TruncateTo(DateTimeExtensions.DateTruncate.Second);

        Content root = new ContentBuilder()
            .WithKey(RootKey)
            .WithContentType(contentType)
            .WithName("Root")
            .WithPropertyValues(
                new
                {
                    title = "The root title",
                    count = 12,
                    datetime = CurrentDateTime,
                    decimalproperty = DecimalValue
                })
            .Build();

        await WaitForIndexing(Cms.Core.Constants.IndexAliases.PublishedContent, () =>
        {
            ContentService.Save(root);
            ContentService.Publish(root, ["*"]);
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
            ContentService.Save(content);
            if (publish)
            {
                ContentService.Publish(content, ["*"]);
            }

            return Task.CompletedTask;
        });
    }
}
