using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Models.Searching;
using Umbraco.Cms.Search.Core.Models.Searching.Faceting;
using Umbraco.Cms.Search.Core.Models.Searching.Filtering;
using Umbraco.Cms.Search.Provider.Examine.Configuration;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Attributes;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Tests.ContentTests.SearchService;

public class InvariantFacetsTests : SearcherTestBase
{
    private IContentType ContentType { get; set; } = null!;

    public static void ConfigureExpandFacetValues(IUmbracoBuilder builder)
        => builder.Services.Configure<SearcherOptions>(options => options.ExpandFacetValues = true);

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanSearchOneIntegerRangeFacet(bool publish)
    {
        await CreateCountDocuments([1, 2, 101]);

        var indexAlias = GetIndexAlias(publish);
        SearchResult result = await Searcher.SearchAsync(indexAlias, null, null, new List<Facet>(){ new IntegerRangeFacet("count", new []{ new IntegerRangeFacetRange("Below 100", 0, 100)})}, null, null, null, null, 0, 100);
        Assert.Multiple(() =>
        {
            Assert.That(result.Facets, Is.Not.Empty);
            Assert.That(result.Facets.First().Values.First().Count, Is.EqualTo(2));
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanSearchIntegerExactFacet(bool publish)
    {
        await CreateCountDocuments([1, 1, 2]);

        var indexAlias = GetIndexAlias(publish);
        IEnumerable<FacetResult> facets = (await Searcher.SearchAsync(indexAlias, null, null, new List<Facet>(){ new IntegerExactFacet("count")}, null, null, null, null, 0, 100)).Facets;
        var firstFacetValues = (IntegerExactFacetValue) facets.First().Values.First();
        var secondFacetValues = (IntegerExactFacetValue) facets.First().Values.Last();
        Assert.Multiple(() =>
        {
            Assert.That(facets, Is.Not.Empty);
            Assert.That(firstFacetValues.Key, Is.EqualTo(1));
            Assert.That(firstFacetValues.Count, Is.EqualTo(2));
            Assert.That(secondFacetValues.Key, Is.EqualTo(2));
            Assert.That(secondFacetValues.Count, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task CanSearchIntegerExactFacetWithFilter()
    {
        await CreateCountDocuments([1, 1, 2, 3, 3]);

        var indexAlias = GetIndexAlias(true);
        IEnumerable<FacetResult> facets = (await Searcher.SearchAsync(
            indexAlias,
            null,
            [new IntegerExactFilter("count", [1], false)],
            [new IntegerExactFacet("count")],
            null,
            null,
            null,
            null,
            0,
            100)).Facets;
        Assert.That(facets.Count(), Is.EqualTo(1));

        FacetResult facet = facets.First();
        Assert.Multiple(() =>
        {
            Assert.That(facet.FieldName, Is.EqualTo("count"));
            Assert.That(facet.Values.Count(), Is.EqualTo(1));
        });

        var facetValue = facet.Values.Single() as IntegerExactFacetValue;
        Assert.That(facetValue, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(facetValue.Key, Is.EqualTo(1));
            Assert.That(facetValue.Count, Is.EqualTo(2));
        });
    }

    [Test]
    [ConfigureUmbracoBuilder(ActionName = nameof(ConfigureExpandFacetValues))]
    public async Task CanSearchIntegerExactFacetExpandedWithFilter()
    {
        await CreateCountDocuments([1, 1, 2, 3, 3]);

        var indexAlias = GetIndexAlias(true);
        IEnumerable<FacetResult> facets = (await Searcher.SearchAsync(
            indexAlias,
            null,
            [new IntegerExactFilter("count", [1], false)],
            [new IntegerExactFacet("count")],
            null,
            null,
            null,
            null,
            0,
            100)).Facets;
        Assert.That(facets.Count(), Is.EqualTo(1));

        FacetResult facet = facets.First();
        Assert.Multiple(() =>
        {
            Assert.That(facet.FieldName, Is.EqualTo("count"));
            Assert.That(facet.Values.Count(), Is.EqualTo(3));
        });

        IntegerExactFacetValue[] facetValues = facet.Values.OfType<IntegerExactFacetValue>().ToArray();
        Assert.That(facetValues, Has.Length.EqualTo(3));

        Assert.Multiple(() =>
        {
            Assert.That(facetValues[0].Key, Is.EqualTo(1));
            Assert.That(facetValues[0].Count, Is.EqualTo(2));

            Assert.That(facetValues[1].Key, Is.EqualTo(2));
            Assert.That(facetValues[1].Count, Is.EqualTo(1));

            Assert.That(facetValues[2].Key, Is.EqualTo(3));
            Assert.That(facetValues[2].Count, Is.EqualTo(2));
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanSearchOneDecimalRangeFacet(bool publish)
    {
        await CreateDecimalDocuments([1.5, 2.5, 100.5]);

        var indexAlias = GetIndexAlias(publish);
        SearchResult result = await Searcher.SearchAsync(indexAlias, null, null, new List<Facet> { new DecimalRangeFacet("decimalproperty", [new DecimalRangeFacetRange("Below 100", 0, 100)]) }, null, null, null, null, 0, 100);
        Assert.Multiple(() =>
        {
            Assert.That(result.Facets, Is.Not.Empty);
            Assert.That(result.Facets.First().Values.First().Count, Is.EqualTo(2));
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanSearchDecimalExactFacet(bool publish)
    {
        await CreateDecimalDocuments([1.55, 1.55, 1.56]);

        var indexAlias = GetIndexAlias(publish);
        IEnumerable<FacetResult> facets = (await Searcher.SearchAsync(indexAlias, null, null, new List<Facet> { new DecimalExactFacet("decimalproperty") }, null, null, null, null, 0, 100)).Facets;
        var firstFacetValues = (DecimalExactFacetValue) facets.First().Values.First();
        var secondFacetValues = (DecimalExactFacetValue) facets.First().Values.Last();
        Assert.Multiple(() =>
        {
            Assert.That(facets, Is.Not.Empty);
            Assert.That(firstFacetValues.Key, Is.EqualTo(1.55));
            Assert.That(firstFacetValues.Count, Is.EqualTo(2));
            Assert.That(secondFacetValues.Key, Is.EqualTo(1.56));
            Assert.That(secondFacetValues.Count, Is.EqualTo(1));
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanSearchKeywordFacet(bool publish)
    {
        await CreateDropDownDocuments(["one", "one", "two"]);

        var indexAlias = GetIndexAlias(publish);
        IEnumerable<FacetResult> facets = (await Searcher.SearchAsync(indexAlias, null, null, new List<Facet> { new KeywordFacet("dropDown") }, null, null, null, null, 0, 100)).Facets;
        var firstFacetValues = (KeywordFacetValue) facets.First().Values.First();
        var secondFacetValues = (KeywordFacetValue) facets.First().Values.Last();
        Assert.Multiple(() =>
        {
            Assert.That(facets, Is.Not.Empty);
            Assert.That(firstFacetValues.Key, Is.EqualTo("one"));
            Assert.That(firstFacetValues.Count, Is.EqualTo(2));
            Assert.That(secondFacetValues.Key, Is.EqualTo("two"));
            Assert.That(secondFacetValues.Count, Is.EqualTo(1));
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanSearchDatetimeRangeFacet(bool publish)
    {
        await CreateDatetimeDocuments([new DateTime(2025, 06, 06), new DateTime(2025, 02, 01), new DateTime(2024, 01, 01)]);

        var indexAlias = GetIndexAlias(publish);
        SearchResult result = await Searcher.SearchAsync(indexAlias, null, null, new List<Facet>(){ new DateTimeOffsetRangeFacet("datetime", [new DateTimeOffsetRangeFacetRange("Below 100", new DateTime(2025, 01, 01), null)])}, null, null, null, null, 0, 100);
        Assert.Multiple(() =>
        {
            Assert.That(result.Facets, Is.Not.Empty);
            Assert.That(result.Facets.First().Values.First().Count, Is.EqualTo(2));
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanSearchDatetimeExactFacet(bool publish)
    {
        var firstDateTime = new DateTime(2025, 06, 06);
        var secondDateTime = new DateTime(2025, 06, 06);
        var thirdDateTime = new DateTime(2025, 06, 01);
        await CreateDatetimeDocuments([firstDateTime, secondDateTime, thirdDateTime]);

        var indexAlias = GetIndexAlias(publish);
        SearchResult result = await Searcher.SearchAsync(indexAlias, null, null, new List<Facet>(){ new DateTimeOffsetExactFacet("datetime")}, null, null, null, null, 0, 100);
        var firstFacetValues = (DateTimeOffsetExactFacetValue) result.Facets.First().Values.First();
        var secondFacetValues = (DateTimeOffsetExactFacetValue) result.Facets.First().Values.Last();
        Assert.Multiple(() =>
        {
            Assert.That(result.Facets, Is.Not.Empty);
            Assert.That(firstFacetValues.Count, Is.EqualTo(1));
            Assert.That(firstFacetValues.Key.DateTime, Is.EqualTo(thirdDateTime));
            Assert.That(secondFacetValues.Count, Is.EqualTo(2));
            Assert.That(secondFacetValues.Key.DateTime, Is.EqualTo(firstDateTime));
        });
    }

    private async Task CreateCountDocType()
    {
        ContentType = new ContentTypeBuilder()
            .WithAlias("invariant")
            .AddPropertyType()
            .WithAlias("count")
            .WithDataTypeId(-51)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Integer)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(ContentType, Constants.Security.SuperUserKey);
    }

    private async Task CreateDatetimeDocType()
    {
        ContentType = new ContentTypeBuilder()
            .WithAlias("invariant")
            .AddPropertyType()
            .WithAlias("datetime")
            .WithDataTypeId(Constants.DataTypes.DateTime)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.DateTime)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(ContentType, Constants.Security.SuperUserKey);
    }

    private async Task CreateDatetimeDocuments(DateTimeOffset[] values)
    {
        await CreateDatetimeDocType();

        await WaitForIndexing(GetIndexAlias(true), () =>
        {
            foreach (DateTimeOffset dateTimeOffset in values)
            {
                Content document = new ContentBuilder()
                    .WithContentType(ContentType)
                    .WithName($"document-{dateTimeOffset.ToString()}")
                    .WithPropertyValues(
                        new {datetime = dateTimeOffset})
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
            .WithAlias(Constants.PropertyEditors.Aliases.Decimal)
            .Done()
            .Build();

        await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        ContentType = new ContentTypeBuilder()
            .WithAlias("invariant")
            .AddPropertyType()
            .WithAlias("decimalproperty")
            .WithDataTypeId(dataType.Id)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Decimal)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(ContentType, Constants.Security.SuperUserKey);
    }

    private async Task CreateDecimalDocuments(double[] values)
    {
        await CreateDecimalDocType();

        await WaitForIndexing(GetIndexAlias(true), () =>
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

        await WaitForIndexing(GetIndexAlias(true), () =>
        {
            foreach (var countValue in values)
            {
                Content document = new ContentBuilder()
                    .WithContentType(ContentType)
                    .WithName($"document-{countValue}")
                    .WithPropertyValues(
                        new
                        {
                            count = countValue,
                        })
                    .Build();

                SaveAndPublish(document);
            }

            return Task.CompletedTask;
        });

    }

    private async Task CreateDropDownDocType()
    {
        ContentType = new ContentTypeBuilder()
            .WithAlias("invariant")
            .AddPropertyType()
            .WithAlias("dropDown")
            .WithDataTypeId(Constants.DataTypes.DropDownSingle)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.DropDownListFlexible)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(ContentType, Constants.Security.SuperUserKey);
    }

    private async Task CreateDropDownDocuments(string[] values)
    {
        await CreateDropDownDocType();

        await WaitForIndexing(GetIndexAlias(true), () =>
        {
            foreach (var stringValue in values)
            {
                Content document = new ContentBuilder()
                    .WithContentType(ContentType)
                    .WithName($"document-{stringValue}")
                    .WithPropertyValues(
                        new {dropDown = $"[\"{stringValue}\"]"})
                    .Build();

                SaveAndPublish(document);
            }

            return Task.CompletedTask;
        });

    }
}
