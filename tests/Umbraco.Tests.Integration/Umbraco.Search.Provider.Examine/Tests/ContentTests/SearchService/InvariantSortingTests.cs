using System.Globalization;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Extensions;
using Umbraco.Cms.Search.Core.Models.Searching;
using Umbraco.Cms.Search.Core.Models.Searching.Filtering;
using Umbraco.Cms.Search.Core.Models.Searching.Sorting;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Tests.ContentTests.SearchService;

public class InvariantSortingTests : SearcherTestBase
{
    private IContentType ContentType { get; set; } = null!;


    [TestCase(Direction.Descending)]
    [TestCase(Direction.Ascending)]
    public async Task CanSortIntegers(Direction direction)
    {
        int[] integers = [9, 3, 4, 2, 5, 7, 1, 11, 10];
        KeyValuePair<Guid, int>[] keys = (await CreateCountDocuments(integers)).OrderBy(x => x.Value, direction).ToArray();

        var indexAlias = GetIndexAlias(true);
        SearchResult result = await Searcher.SearchAsync(
            indexAlias,
            null,
            [new IntegerRangeFilter("count", [new IntegerRangeFilterRange(null, null)], false)],
            null,
            [new IntegerSorter("count", direction)],
            null,
            null,
            null,
            0,
            100);

        Document[] documents = result.Documents.ToArray();
        Assert.That(documents, Has.Length.EqualTo(keys.Length));
        for (var i = 0; i < keys.Length; i++)
        {
            Assert.That(documents[i].Id, Is.EqualTo(keys[i].Key));
        }
    }

    [TestCase(true, Direction.Descending)]
    [TestCase(false, Direction.Ascending)]
    public async Task CanSortDecimals(bool publish, Direction direction)
    {
        double[] doubles = [5, 12412d, 0, 51251d, 1.15215d, 3.251d, 2.251512d, 125.5215d, 142.214124d];
        KeyValuePair<Guid, double>[] keys = (await CreateDecimalDocuments(doubles)).OrderBy(x => x.Value, direction).ToArray();

        var indexAlias = GetIndexAlias(publish);
        SearchResult result = await Searcher.SearchAsync(
            indexAlias,
            null,
            [new DecimalRangeFilter("decimalproperty", [new DecimalRangeFilterRange(null, null)], false)],
            null,
            [new DecimalSorter("decimalproperty", direction)],
            null,
            null,
            null,
            0,
            100);

        Document[] documents = result.Documents.ToArray();
        Assert.That(documents, Has.Length.EqualTo(keys.Length));
        for (int i = 0; i < keys.Length; i++)
        {
            Assert.That(documents[i].Id, Is.EqualTo(keys[i].Key));
        }
    }

    [TestCase(true, Direction.Descending)]
    [TestCase(false, Direction.Ascending)]
    public async Task CanSortDateTimeOffsets(bool publish, Direction direction)
    {
        DateTime[] dateTimes = [new(2025, 06, 06), new(2025, 02, 01), new(2024, 01, 01), new(2019, 01, 01), new(2000, 01, 01), new(2003, 01, 01)];

        KeyValuePair<Guid, DateTime>[] keys = (await CreateDatetimeDocuments(dateTimes)).OrderBy(x => x.Value, direction).ToArray();

        var indexAlias = GetIndexAlias(publish);
        SearchResult result = await Searcher.SearchAsync(
            indexAlias,
            null,
            [new DateTimeOffsetRangeFilter("datetime", [new DateTimeOffsetRangeFilterRange(null, null)], false)],
            null,
            [new DateTimeOffsetSorter("datetime", direction)],
            null,
            null,
            null,
            0,
            100);

        Document[] documents = result.Documents.ToArray();
        Assert.That(documents, Has.Length.EqualTo(keys.Length));
        for (int i = 0; i < keys.Length; i++)
        {
            Assert.That(documents[i].Id, Is.EqualTo(keys[i].Key));
        }
    }

    [TestCase(true, Direction.Descending)]
    [TestCase(false, Direction.Ascending)]
    public async Task CanSortKeywords(bool publish, Direction direction)
    {
        KeyValuePair<Guid, string>[] keysAndValues = (await CreateDropDownDocuments(["r", "f", "a", "m", "x"])).ToArray();

        var indexAlias = GetIndexAlias(publish);
        SearchResult result = await Searcher.SearchAsync(
            indexAlias,
            null,
            [new KeywordFilter("Umb_Id", keysAndValues.Select(kvp => kvp.Key.AsKeyword()).ToArray(), false)],
            null,
            [new KeywordSorter("dropDown", direction)],
            null,
            null,
            null,
            0,
            100);

        Document[] documents = result.Documents.ToArray();
        Assert.That(documents, Has.Length.EqualTo(keysAndValues.Length));
        Guid[] orderedKeys = (direction == Direction.Ascending
                ? keysAndValues.OrderBy(kvp => kvp.Value)
                : keysAndValues.OrderByDescending(kvp => kvp.Value))
            .Select(kvp => kvp.Key)
            .ToArray();

        for (var i = 0; i < orderedKeys.Length; i++)
        {
            Assert.That(documents[i].Id, Is.EqualTo(orderedKeys[i]));
        }
    }

    [TestCase(true, Direction.Descending)]
    [TestCase(false, Direction.Ascending)]
    public async Task CanSortTexts(bool publish, Direction direction)
    {
        string[] titles = ["abc", "ccc", "bbb", "ddd", "zzz", "xxx"];

        KeyValuePair<Guid, string>[] keys = (await CreateTitleDocuments(titles)).OrderBy(x => x.Value, direction).ToArray();

        var indexAlias = GetIndexAlias(publish);
        SearchResult result = await Searcher.SearchAsync(
            indexAlias,
            null,
            [new TextFilter("title", titles, false)],
            null,
            [new TextSorter("title", direction)],
            null,
            null,
            null,
            0,
            100);

        Document[] documents = result.Documents.ToArray();
        Assert.That(documents, Has.Length.EqualTo(keys.Length));
        for (int i = 0; i < keys.Length; i++)
        {
            Assert.That(documents[i].Id, Is.EqualTo(keys[i].Key));
        }
    }

    [TestCase(true, Direction.Descending)]
    [TestCase(false, Direction.Ascending)]
    public async Task CanSortByDocumentName(bool publish, Direction direction)
    {
        string[] titles = ["xxx", "zzz", "yyy", "www"];

        KeyValuePair<Guid, string>[] keys = (await CreateTitleDocuments(titles))
            .OrderBy(x => x.Value, direction)
            .ToArray();

        var indexAlias = GetIndexAlias(publish);
        SearchResult result = await Searcher.SearchAsync(
            indexAlias,
            null,
            null,
            null,
            [new TextSorter(Cms.Search.Core.Constants.FieldNames.Name, direction)],
            null,
            null,
            null,
            0,
            100);

        Document[] documents = result.Documents.ToArray();
        Assert.That(documents, Has.Length.EqualTo(keys.Length));
        for (var i = 0; i < keys.Length; i++)
        {
            Assert.That(documents[i].Id, Is.EqualTo(keys[i].Key));
        }
    }

    // TODO: Remake these with actual properties in different r1 texts...
    // [TestCase(true, Direction.Descending)]
    // [TestCase(false, Direction.Ascending)]
    // public async Task CanSortByScore(bool publish, Direction direction)
    // {
    //     string[] titles = ["exact", "exact aa ", "exact aaa aaaa", "exact aaa aaaa aaaaa"];
    //
    //     var keys = (await CreateTitleDocuments(titles)).OrderBy(x => x.Value, direction).ToArray();
    //
    //     var indexAlias = GetIndexAlias(publish);
    //     var result = await Searcher.SearchAsync(
    //         indexAlias,
    //         "exact",
    //         null,
    //         null,
    //         [new ScoreSorter(direction)],
    //         null,
    //         null,
    //         null,
    //         0,
    //         100);
    //
    //     Assert.Multiple(() =>
    //     {
    //         var documents = result.Documents.ToArray();
    //         Assert.That(documents, Is.Not.Empty);
    //         for (int i = 0; i < keys.Length; i++)
    //         {
    //             Assert.That(documents[i].Id, Is.EqualTo(keys[i].Key));
    //         }
    //     });
    // }


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

    private async Task<IEnumerable<KeyValuePair<Guid, string>>> CreateDropDownDocuments(string[] values)
    {
        var keysAndValues = new List<KeyValuePair<Guid, string>>();
        await CreateDropDownDocType();

        await WaitForIndexing(GetIndexAlias(true), () =>
        {
            foreach (var stringValue in values)
            {
                Content document = new ContentBuilder()
                    .WithContentType(ContentType)
                    .WithName($"document-{stringValue}")
                    .WithPropertyValues(
                        new { dropDown = $"[\"{stringValue}\"]" })
                    .Build();

                SaveAndPublish(document);
                keysAndValues.Add(new KeyValuePair<Guid, string>(document.Key, stringValue));
            }

            return Task.CompletedTask;
        });

        return keysAndValues;
    }

    private async Task CreateTitleDocType()
    {
        ContentType = new ContentTypeBuilder()
            .WithAlias("invariant")
            .AddPropertyType()
            .WithAlias("title")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(ContentType, Constants.Security.SuperUserKey);
    }

    private async Task<Dictionary<Guid, string>> CreateTitleDocuments(string[] values)
    {
        var keys = new Dictionary<Guid, string>();
        await CreateTitleDocType();

        await WaitForIndexing(GetIndexAlias(true), () =>
        {
            foreach (var stringValue in values)
            {
                Content document = new ContentBuilder()
                    .WithContentType(ContentType)
                    .WithName($"document-{stringValue}")
                    .WithPropertyValues(
                        new { title = stringValue })
                    .Build();

                ContentService.Save(document);
                ContentService.Publish(document, new[] { "*" });
                keys.Add(document.Key, stringValue);
            }

            return Task.CompletedTask;
        });

        return keys;
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

    private async Task<Dictionary<Guid, DateTime>> CreateDatetimeDocuments(DateTime[] values)
    {
        var keys = new Dictionary<Guid, DateTime>();
        await CreateDatetimeDocType();

        await WaitForIndexing(GetIndexAlias(true), () =>
        {
            foreach (DateTime dateTimeOffset in values)
            {
                Content document = new ContentBuilder()
                    .WithContentType(ContentType)
                    .WithName($"document-{dateTimeOffset.ToString(CultureInfo.InvariantCulture)}")
                    .WithPropertyValues(
                        new
                        {
                            datetime = dateTimeOffset
                        })
                    .Build();

                ContentService.Save(document);
                ContentService.Publish(document, new[] { "*" });
                keys.Add(document.Key, dateTimeOffset);
            }

            return Task.CompletedTask;
        });

        return keys;
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

    private async Task<Dictionary<Guid, double>> CreateDecimalDocuments(double[] values)
    {
        var keys = new Dictionary<Guid, double>();
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

                ContentService.Save(document);
                ContentService.Publish(document, new[] { "*" });
                keys.Add(document.Key, doubleValue);
            }

            return Task.CompletedTask;
        });

        return keys;
    }

    private async Task<Dictionary<Guid, int>> CreateCountDocuments(int[] values)
    {
        var keys = new Dictionary<Guid, int>();
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

                ContentService.Save(document);
                ContentService.Publish(document, new[] { "*" });
                keys.Add(document.Key, countValue);
            }

            return Task.CompletedTask;
        });

        return keys;
    }
}
