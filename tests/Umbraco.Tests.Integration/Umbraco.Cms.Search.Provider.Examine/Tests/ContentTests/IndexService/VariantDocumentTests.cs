using System.Globalization;
using Examine;
using Examine.Search;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Search.Indexing;
using Umbraco.Cms.Search.Provider.Examine.Helpers;
using Umbraco.Cms.Tests.Common.Attributes;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Constants = Umbraco.Cms.Search.Provider.Examine.Constants;
using CoreConstants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Tests.ContentTests.IndexService;

[LongRunning]
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
        IIndex index = GetIndex(indexAlias);
        ISearchResults results = index.Searcher.CreateQuery().All().Execute();
        Assert.That(results.TotalItemCount, Is.EqualTo(3));

        await WaitForIndexing(indexAlias, () =>
        {
            IContent content = ContentService.GetById(RootKey)!;
            ContentService.Unpublish(content, "da-DK");
            return Task.CompletedTask;
        });

        results = index.Searcher.CreateQuery().All().Execute();
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
        IIndex index = GetIndex(CoreConstants.Search.IndexAliases.PublishedContent);

        ISearchResults results = index.Searcher.CreateQuery().All().Execute();

        // the index stores values culture invariantly, so the expected value must be formatted the same way
        var expectedValue = Convert.ToString(value, CultureInfo.InvariantCulture);
        ISearchResult result = results.Single(x => x.Id == DocumentIdHelper.CalculateDocumentId(RootKey, culture));
        Assert.That(result.Values[field], Is.EqualTo(expectedValue));
    }

    [TestCase("en-US", "1.5", "2026-01-02T03:04:05.0000000")]
    [TestCase("da-DK", "-2.25", "2026-02-03T04:05:06.0000000")]
    [TestCase("ja-JP", "3.75", "2026-03-04T05:06:07.0000000")]
    public async Task CanIndexVariantNumericAndDateValuesByCultureIndependentOfServerCulture(string culture, string expectedDecimal, string expectedDate)
    {
        // "," as the decimal separator, U+2212 as the negative sign.
        await WithServerCulture("sv-SE", CreateVariantDocument);

        ISearchResult result = GetIndex(CoreConstants.Search.IndexAliases.PublishedContent).Searcher.CreateQuery().All().Execute()
            .Single(x => x.Id == DocumentIdHelper.CalculateDocumentId(RootKey, culture));
        Assert.Multiple(() =>
        {
            Assert.That(result.Values[FieldNameHelper.FieldName("variantdecimalproperty", Constants.FieldValues.Decimals)], Is.EqualTo(expectedDecimal));
            Assert.That(result.Values[FieldNameHelper.FieldName("variantdate", Constants.FieldValues.DateTimeOffsets)], Is.EqualTo(expectedDate));
        });
    }

    [TestCase("en-US")]
    [TestCase("de-DE")] // "," as the decimal separator, day-first dates.
    [TestCase("sv-SE")] // U+2212 as the negative sign.
    public async Task CanIndexValuesOfFieldsWithoutDefinitionIndependentOfCurrentCulture(string currentCulture)
    {
        const string indexAlias = CoreConstants.Search.IndexAliases.PublishedContent;
        const string propertyName = "fieldWithoutDefinition";
        var key = Guid.NewGuid();

        await WithServerCulture(currentCulture, () => WaitForIndexing(indexAlias, () => Indexer.AddOrUpdateAsync(
            indexAlias,
            key,
            UmbracoObjectTypes.Document,
            [new Variation(Culture: null, Segment: null)],
            [
                new IndexField(
                    propertyName,
                    new IndexValue
                    {
                        Integers = [-12],
                        Decimals = [-12.4552m],
                        DateTimeOffsets = [new DateTimeOffset(2026, 11, 4, 14, 30, 15, TimeSpan.Zero)],
                    },
                    Culture: null,
                    Segment: null),
            ],
            null)));

        ISearchResult result = GetIndex(indexAlias).Searcher.CreateQuery().All().Execute()
            .Single(x => x.Id == DocumentIdHelper.CalculateDocumentId(key, null));
        Assert.Multiple(() =>
        {
            Assert.That(result.Values[FieldNameHelper.FieldName(propertyName, Constants.FieldValues.Integers)], Is.EqualTo("-12"));
            Assert.That(result.Values[FieldNameHelper.FieldName(propertyName, Constants.FieldValues.Decimals)], Is.EqualTo("-12.4552"));
            Assert.That(result.Values[FieldNameHelper.FieldName(propertyName, Constants.FieldValues.DateTimeOffsets)], Is.EqualTo("2026-11-04T14:30:15.0000000"));
        });
    }

    [TestCase("title", "updatedTitle", "en-US")]
    [TestCase("title", "updatedTitle", "da-DK")]
    [TestCase("title", "updatedTitle", "ja-JP")]
    public async Task CanIndexUpdatedProperties(string propertyName, string updatedValue, string culture)
    {
        await CreateVariantDocument();
        await UpdateProperty(propertyName, updatedValue, culture);

        IIndex index = GetIndex(CoreConstants.Search.IndexAliases.PublishedContent);

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
    [TestCase(true, "da-DK", "segment-1", "krop-segment-1")]
    [TestCase(false, "da-DK", "segment-2", "krop-segment-2")]
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
            .WithAlias(CoreConstants.PropertyEditors.Aliases.Decimal)
            .Done()
            .Build();

        await DataTypeService.CreateAsync(dataType, CoreConstants.Security.SuperUserKey);

        ILanguage langDk = new LanguageBuilder()
            .WithCultureInfo("da-DK")
            .WithIsDefault(true)
            .Build();
        ILanguage langJp = new LanguageBuilder()
            .WithCultureInfo("ja-JP")
            .Build();

        await LanguageService.CreateAsync(langDk, CoreConstants.Security.SuperUserKey);
        await LanguageService.CreateAsync(langJp, CoreConstants.Security.SuperUserKey);

        IContentType contentType = new ContentTypeBuilder()
            .WithAlias("variant")
            .WithContentVariation(ContentVariation.CultureAndSegment)
            .AddPropertyType()
            .WithAlias("title")
            .WithVariations(ContentVariation.Culture)
            .WithDataTypeId(CoreConstants.DataTypes.Textbox)
            .WithPropertyEditorAlias(CoreConstants.PropertyEditors.Aliases.TextBox)
            .Done()
            .AddPropertyType()
            .WithAlias("invarianttitle")
            .WithVariations(ContentVariation.Nothing)
            .WithDataTypeId(CoreConstants.DataTypes.Textbox)
            .WithPropertyEditorAlias(CoreConstants.PropertyEditors.Aliases.TextBox)
            .Done()
            .AddPropertyType()
            .WithAlias("invariantcount")
            .WithVariations(ContentVariation.Nothing)
            .WithDataTypeId(-51)
            .WithPropertyEditorAlias(CoreConstants.PropertyEditors.Aliases.Integer)
            .Done()
            .AddPropertyType()
            .WithAlias("invariantdecimalproperty")
            .WithVariations(ContentVariation.Nothing)
            .WithDataTypeId(dataType.Id)
            .WithPropertyEditorAlias(CoreConstants.PropertyEditors.Aliases.Decimal)
            .Done()
            .AddPropertyType()
            .WithAlias("variantdecimalproperty")
            .WithVariations(ContentVariation.Culture)
            .WithDataTypeId(dataType.Id)
            .WithPropertyEditorAlias(CoreConstants.PropertyEditors.Aliases.Decimal)
            .Done()
            .AddPropertyType()
            .WithAlias("variantdate")
            .WithVariations(ContentVariation.Culture)
            .WithDataTypeId(CoreConstants.DataTypes.DateTime)
            .WithPropertyEditorAlias(CoreConstants.PropertyEditors.Aliases.DateTime)
            .Done()
            .AddPropertyType()
            .WithAlias("body")
            .WithVariations(ContentVariation.CultureAndSegment)
            .WithDataTypeId(CoreConstants.DataTypes.Textbox)
            .WithPropertyEditorAlias(CoreConstants.PropertyEditors.Aliases.TextBox)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(contentType, CoreConstants.Security.SuperUserKey);

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
        root.SetValue("variantdecimalproperty", 1.5, "en-US");
        root.SetValue("variantdecimalproperty", -2.25, "da-DK");
        root.SetValue("variantdecimalproperty", 3.75, "ja-JP");
        root.SetValue("variantdate", new DateTime(2026, 1, 2, 3, 4, 5), "en-US");
        root.SetValue("variantdate", new DateTime(2026, 2, 3, 4, 5, 6), "da-DK");
        root.SetValue("variantdate", new DateTime(2026, 3, 4, 5, 6, 7), "ja-JP");

        root.SetValue("body", "body-segment-1", "en-US", "segment-1");
        root.SetValue("body", "body-segment-2", "en-US", "segment-2");
        root.SetValue("body", "krop-segment-1", "da-DK", "segment-1");
        root.SetValue("body", "krop-segment-2", "da-DK", "segment-2");
        root.SetValue("body", "ボディ-segment-1", "ja-JP", "segment-1");
        root.SetValue("body", "ボディ-segment-2", "ja-JP", "segment-2");

        await WaitForIndexing(CoreConstants.Search.IndexAliases.PublishedContent, () =>
        {
            ContentService.Save(root);
            ContentService.Publish(root, ["*"]);
            return Task.CompletedTask;
        });

        IContent? content = ContentService.GetById(RootKey);
        Assert.That(content, Is.Not.Null);
    }


    private static async Task WithServerCulture(string culture, Func<Task> action)
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        CultureInfo? originalDefaultThreadCulture = CultureInfo.DefaultThreadCurrentCulture;
        try
        {
            // indexing runs on background threads, which take the default thread culture
            CultureInfo.CurrentCulture = CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(culture);
            await action();
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.DefaultThreadCurrentCulture = originalDefaultThreadCulture;
        }
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
