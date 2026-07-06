using Examine;
using Examine.Search;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Provider.Examine.Helpers;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Constants = Umbraco.Cms.Search.Provider.Examine.Constants;

namespace Umbraco.Tests.Search.Examine.Integration.Tests.ContentTests.IndexService;

public class VariantDocumentTests : IndexTestBase
{
    [TestCase(true)]
    [TestCase(false)]
    public async Task CanIndexAnyDocument(bool publish)
    {
        await CreateVariantDocument();
        IIndex index = GetIndex(GetIndexAlias(publish));

        ISearchResults results = index.Searcher.CreateQuery().All().Execute();
        Assert.That(results, Is.Not.Empty);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanRemoveAnyDocument(bool publish)
    {
        await CreateVariantDocument();
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
    public async Task CanRemoveSingleCultureFromPublishedDocument()
    {
        await CreateVariantDocument();
        var indexAlias = GetIndexAlias(true);
        await WaitForIndexing(indexAlias, () =>
        {
            IContent content = ContentService.GetById(RootKey)!;
            ContentService.Unpublish(content, "da-DK");
            return Task.CompletedTask;
        });

        IIndex index = GetIndex(indexAlias);
        ISearchResults results = index.Searcher.CreateQuery().All().Execute();
        Assert.That(results.TotalItemCount, Is.EqualTo(2));
    }

    [TestCase(true, "en-US", "Name")]
    [TestCase(false, "en-US", "Name")]
    [TestCase(true, "da-DK", "Navn")]
    [TestCase(false, "da-DK", "Navn")]
    [TestCase(true, "ja-JP", "名前")]
    [TestCase(false, "ja-JP", "名前")]
    public async Task CanIndexVariantName(bool publish, string culture, string expectedValue)
    {
        await CreateVariantDocument();
        IIndex index = GetIndex(GetIndexAlias(publish));

        IOrdering queryBuilder = index.Searcher.CreateQuery().All();
        queryBuilder.SelectField(Constants.SystemFields.AggregatedTextsR1);
        ISearchResults results = queryBuilder.Execute();
        var result = results
            .SelectMany(x => x.Values.Values)
            .First(x => x == expectedValue);
        Assert.That(results, Is.Not.Empty);
        Assert.That(result, Is.EqualTo(expectedValue));
    }

    [TestCase("invarianttitle", Constants.FieldValues.Texts, "Invariant", "en-US")]
    [TestCase("invarianttitle", Constants.FieldValues.Texts, "Invariant", "da-DK")]
    [TestCase("invarianttitle", Constants.FieldValues.Texts, "Invariant", "ja-JP")]
    [TestCase("invariantcount", Constants.FieldValues.Integers, 12, "en-US")]
    [TestCase("invariantcount", Constants.FieldValues.Integers, 12, "da-DK")]
    [TestCase("invariantcount", Constants.FieldValues.Integers, 12, "ja-JP")]
    [TestCase("invariantdecimalproperty", Constants.FieldValues.Decimals, 12.4552, "en-US")]
    [TestCase("invariantdecimalproperty", Constants.FieldValues.Decimals, 12.4552, "da-DK")]
    [TestCase("invariantdecimalproperty", Constants.FieldValues.Decimals, 12.4552, "ja-JP")]
    public async Task CanIndexInvariantProperty(string property, string fieldValues, object value, string culture)
    {
        await CreateVariantDocument();
        var field = FieldNameHelper.FieldName(property, fieldValues);
        IIndex index = GetIndex(Cms.Search.Core.Constants.IndexAliases.PublishedContent);

        IOrdering queryBuilder = index.Searcher.CreateQuery().All();
        queryBuilder.SelectField(field);
        ISearchResults results = queryBuilder.Execute();
        var result = results
            .SelectMany(x => x.Values.Values)
            .First(x => x == value.ToString());
        Assert.That(results, Is.Not.Empty);
        Assert.That(result, Is.EqualTo(value.ToString()));
    }

    [TestCase("title", "updatedTitle", "en-US")]
    [TestCase("title", "updatedTitle", "da-DK")]
    [TestCase("title", "updatedTitle", "ja-JP")]
    public async Task CanIndexUpdatedProperties(string propertyName, string updatedValue, string culture)
    {
        await CreateVariantDocument();
        await UpdateProperty(propertyName, updatedValue, culture);

        IIndex index = GetIndex(Cms.Search.Core.Constants.IndexAliases.PublishedContent);

        ISearchResults results = index.Searcher.Search(updatedValue);
        Assert.That(results, Is.Not.Empty);
        var fieldName = FieldNameHelper.FieldName(propertyName, Constants.FieldValues.Texts);
        Assert.That(results.First().Values.First(x => x.Key == fieldName).Value, Is.EqualTo(updatedValue));
    }

    [TestCase(true, "en-US", "Root")]
    [TestCase(false, "en-US", "Root")]
    [TestCase(true, "da-DK", "Rod")]
    [TestCase(false, "da-DK", "Rod")]
    [TestCase(true, "ja-JP", "ル-ト")]
    [TestCase(false, "ja-JP", "ル-ト")]
    public async Task CanIndexVariantTextByCulture(bool publish, string culture, string expectedValue)
    {
        await CreateVariantDocument();
        IIndex index = GetIndex(GetIndexAlias(publish));

        IOrdering queryBuilder = index.Searcher.CreateQuery().All();
        var fieldName = FieldNameHelper.FieldName("title", Constants.FieldValues.Texts);
        queryBuilder.SelectField(fieldName);
        ISearchResults results = queryBuilder.Execute();

        Assert.That(results, Is.Not.Empty);
        Assert.That(results.SelectMany(r => r.Values.Values).Contains(expectedValue), Is.True);
    }

    [TestCase(true, "en-US", "segment-1", "body-segment-1")]
    [TestCase(false, "en-US", "segment-2", "body-segment-2")]
    [TestCase(true, "da-DK","segment-1", "krop-segment-1")]
    [TestCase(false, "da-DK","segment-2", "krop-segment-2")]
    [TestCase(true, "ja-JP", "segment-1", "ボディ-segment-1")]
    [TestCase(false, "ja-JP", "segment-2", "ボディ-segment-2")]
    public async Task CanIndexVariantTextBySegment(bool publish, string culture, string segment, string expectedValue)
    {
        await CreateVariantDocument();
        IIndex index = GetIndex(GetIndexAlias(publish));

        ISearchResults results = index.Searcher.Search(expectedValue);
        Assert.That(results, Is.Not.Empty);
        var fieldName = FieldNameHelper.FieldName("body", Constants.FieldValues.Texts, segment);
        Assert.That(results.First().Values.First(x => x.Key == fieldName).Value, Is.EqualTo(expectedValue));
    }

    private async Task CreateVariantDocument()
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

        ILanguage langDk = new LanguageBuilder()
            .WithCultureInfo("da-DK")
            .WithIsDefault(true)
            .Build();
        ILanguage langJp = new LanguageBuilder()
            .WithCultureInfo("ja-JP")
            .Build();

        await LanguageService.CreateAsync(langDk, Cms.Core.Constants.Security.SuperUserKey);
        await LanguageService.CreateAsync(langJp, Cms.Core.Constants.Security.SuperUserKey);

        IContentType contentType = new ContentTypeBuilder()
            .WithAlias("variant")
            .WithContentVariation(ContentVariation.CultureAndSegment)
            .AddPropertyType()
            .WithAlias("title")
            .WithVariations(ContentVariation.Culture)
            .WithDataTypeId(Cms.Core.Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Cms.Core.Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .AddPropertyType()
            .WithAlias("invarianttitle")
            .WithVariations(ContentVariation.Nothing)
            .WithDataTypeId(Cms.Core.Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Cms.Core.Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .AddPropertyType()
            .WithAlias("invariantcount")
            .WithVariations(ContentVariation.Nothing)
            .WithDataTypeId(-51)
            .WithPropertyEditorAlias(Cms.Core.Constants.PropertyEditors.Aliases.Integer)
            .Done()
            .AddPropertyType()
            .WithAlias("invariantdecimalproperty")
            .WithVariations(ContentVariation.Nothing)
            .WithDataTypeId(dataType.Id)
            .WithPropertyEditorAlias(Cms.Core.Constants.PropertyEditors.Aliases.Decimal)
            .Done()
            .AddPropertyType()
            .WithAlias("body")
            .WithVariations(ContentVariation.CultureAndSegment)
            .WithDataTypeId(Cms.Core.Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Cms.Core.Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(contentType, Cms.Core.Constants.Security.SuperUserKey);

        Content root = new ContentBuilder()
            .WithKey(RootKey)
            .WithContentType(contentType)
            .WithCultureName("en-US", "Name")
            .WithCultureName("da-DK", "Navn")
            .WithCultureName("ja-JP", "名前")
            .Build();

        root.SetValue("invarianttitle", "Invariant");
        root.SetValue("invariantcount", 12);
        root.SetValue("invariantdecimalproperty", 12.4552);
        root.SetValue("title", "Root", "en-US");
        root.SetValue("title", "Rod", "da-DK");
        root.SetValue("title", "ル-ト", "ja-JP");

        root.SetValue("body", "body-segment-1", "en-US", "segment-1");
        root.SetValue("body", "body-segment-2", "en-US", "segment-2");
        root.SetValue("body", "krop-segment-1", "da-DK", "segment-1");
        root.SetValue("body", "krop-segment-2", "da-DK", "segment-2");
        root.SetValue("body", "ボディ-segment-1", "ja-JP", "segment-1");
        root.SetValue("body", "ボディ-segment-2", "ja-JP", "segment-2");

        await WaitForIndexing(Cms.Search.Core.Constants.IndexAliases.PublishedContent, () =>
        {
            ContentService.Save(root);
            ContentService.Publish(root, ["*"]);
            return Task.CompletedTask;
        });

        IContent? content = ContentService.GetById(RootKey);
        Assert.That(content, Is.Not.Null);
    }


    private async Task UpdateProperty(string propertyName, object value, string culture)
    {
        IContent content = ContentService.GetById(RootKey)!;
        content.SetValue(propertyName, value, culture);

        await WaitForIndexing(GetIndexAlias(true), () =>
        {
            ContentService.Save(content);
            ContentService.Publish(content, ["*"]);
            return Task.CompletedTask;
        });
    }
}
