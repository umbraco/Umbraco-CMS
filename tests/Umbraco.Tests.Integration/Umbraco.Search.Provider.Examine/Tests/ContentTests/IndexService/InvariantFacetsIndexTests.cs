using System.Globalization;
using Examine;
using Examine.Lucene;
using Examine.Search;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Provider.Examine.Helpers;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Constants = Umbraco.Cms.Search.Provider.Examine.Constants;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Tests.ContentTests.IndexService;

public class InvariantFacetsIndexTests : IndexTestBase
{
    private IContentType ContentType { get; set; } = null!;

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanGetOneIntFacet(bool publish)
    {
        await CreateCountDocuments([1, 2]);

        IIndex index = GetIndex(publish
            ? Cms.Core.Constants.IndexAliases.PublishedContent
            : Cms.Core.Constants.IndexAliases.DraftContent);

        var fieldName = FieldNameHelper.FieldName("otherName", Constants.FieldValues.Integers);
        ISearchResults results = index.Searcher.CreateQuery()
            .All()
            .WithFacets(facets => facets.FacetLongRange(fieldName, new Int64Range("0-9", 0, true, 9, true)))
            .Execute();

        IFacetResult[] facets = results.GetFacets().ToArray();
        IFacetValue? facet = facets.First().Facet("0-9");
        Assert.Multiple(() =>
        {
            Assert.That(facets, Is.Not.Empty);
            Assert.That(facet!.Value, Is.EqualTo(2));
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanGetOneDecimalFacet(bool publish)
    {
        await CreateDecimalDocuments([3.6, 600.4]);

        IIndex index = GetIndex(publish
            ? Cms.Core.Constants.IndexAliases.PublishedContent
            : Cms.Core.Constants.IndexAliases.DraftContent);

        var fieldName = FieldNameHelper.FieldName("decimalproperty", Constants.FieldValues.Decimals);
        ISearchResults results = index.Searcher.CreateQuery()
            .All()
            .WithFacets(facets => facets.FacetDoubleRange(fieldName, new DoubleRange("values", 3.5, true, 654.9, true)))
            .Execute();

        IFacetResult[] facets = results.GetFacets().ToArray();
        IFacetValue? facet = facets.First().Facet("values");
        Assert.Multiple(() =>
        {
            Assert.That(facets, Is.Not.Empty);
            Assert.That(facet!.Value, Is.EqualTo(2));
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanGetOneTextFacet(bool publish)
    {
        await CreateTitleDocuments(["Title", "Title", "Another"]);

        IIndex index = GetIndex(publish
            ? Cms.Core.Constants.IndexAliases.PublishedContent
            : Cms.Core.Constants.IndexAliases.DraftContent);

        var fieldName = FieldNameHelper.FieldName("title", Constants.FieldValues.Texts);
        ISearchResults results = index.Searcher.CreateQuery()
            .All()
            .WithFacets(facets => facets.FacetString(fieldName))
            .Execute();

        IFacetResult[] facets = results.GetFacets().ToArray();
        IFacetValue? facet = facets.First().Facet("Title");
        Assert.Multiple(() =>
        {
            Assert.That(facets, Is.Not.Empty);
            Assert.That(facet!.Value, Is.EqualTo(2));
        });
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task CanGetMultipleIntFacets(bool publish)
    {
        await CreateCountDocuments([1, 2, 99, 101, 170]);

        IIndex index = GetIndex(publish
            ? Cms.Core.Constants.IndexAliases.PublishedContent
            : Cms.Core.Constants.IndexAliases.DraftContent);

        var fieldName = FieldNameHelper.FieldName("otherName", Constants.FieldValues.Integers);
        ISearchResults results = index.Searcher.CreateQuery()
            .All()
            .WithFacets(facets => facets.FacetLongRange(fieldName, new Int64Range("0-9", 0, true, 9, true),  new Int64Range("100-199", 100, true, 199, true)))
            .Execute();

        IFacetResult[] facets = results.GetFacets().ToArray();
        IFacetValue? firstFacet = facets.First().Facet("0-9");
        IFacetValue? secondFacet = facets.First().Facet("100-199");
        Assert.Multiple(() =>
        {
            Assert.That(firstFacet!.Value, Is.EqualTo(2));
            Assert.That(secondFacet!.Value, Is.EqualTo(2));
        });
    }

    private async Task CreateCountDocType()
    {
        ContentType = new ContentTypeBuilder()
            .WithAlias("invariant")
            .AddPropertyType()
            .WithAlias("otherName")
            .WithDataTypeId(-51)
            .WithPropertyEditorAlias(Cms.Core.Constants.PropertyEditors.Aliases.Integer)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(ContentType, Cms.Core.Constants.Security.SuperUserKey);
    }

    private async Task CreateTitleDocType()
    {
        ContentType = new ContentTypeBuilder()
            .WithAlias("invariant")
            .AddPropertyType()
            .WithAlias("title")
            .WithDataTypeId(Cms.Core.Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Cms.Core.Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(ContentType, Cms.Core.Constants.Security.SuperUserKey);
    }

    private async Task CreateTitleDocuments(string[] values)
    {
        await CreateTitleDocType();

        await WaitForIndexing(Cms.Core.Constants.IndexAliases.PublishedContent, () =>
        {
            foreach (var stringValue in values)
            {
                Content document = new ContentBuilder()
                    .WithContentType(ContentType)
                    .WithName($"document-{stringValue}")
                    .WithPropertyValues(
                        new
                        {
                            title = stringValue
                        })
                    .Build();

                SaveAndPublish(document);
            }

            return Task.CompletedTask;
        });
    }

    private async Task CreateDecimalDocType()
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
        ContentType = new ContentTypeBuilder()
            .WithAlias("invariant")
            .AddPropertyType()
            .WithAlias("decimalproperty")
            .WithDataTypeId(dataType.Id)
            .WithPropertyEditorAlias(Cms.Core.Constants.PropertyEditors.Aliases.Decimal)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(ContentType, Cms.Core.Constants.Security.SuperUserKey);
    }

    private async Task CreateDecimalDocuments(double[] values)
    {
        await CreateDecimalDocType();

        await WaitForIndexing(Cms.Core.Constants.IndexAliases.PublishedContent, () =>
        {
            foreach (var doubleValue in values)
            {
                Content document = new ContentBuilder()
                    .WithContentType(ContentType)
                    .WithName($"document-{doubleValue.ToString(CultureInfo.InvariantCulture)}")
                    .WithPropertyValues(
                        new
                        {
                            decimalproperty = doubleValue
                        })
                    .Build();

                SaveAndPublish(document);
            }

            return Task.CompletedTask;
        });
    }

    private async Task CreateCountDocuments(int[] values)
    {
        await CreateCountDocType();

        await WaitForIndexing(Cms.Core.Constants.IndexAliases.PublishedContent, () =>
        {
            foreach (var countValue in values)
            {
                Content document = new ContentBuilder()
                    .WithContentType(ContentType)
                    .WithName($"document-{countValue}")
                    .WithPropertyValues(
                        new
                        {
                            otherName = countValue,
                        })
                    .Build();

                SaveAndPublish(document);
            }

            return Task.CompletedTask;
        });
    }
}
