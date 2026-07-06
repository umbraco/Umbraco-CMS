using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Configuration;
using Umbraco.Cms.Search.Core.Extensions;
using Umbraco.Cms.Search.Core.Models.Configuration;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Cms.Search.Core.Models.Searching;
using Umbraco.Cms.Search.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Tests.Search.Integration.Services;

namespace Umbraco.Tests.Search.Integration.Tests;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class CustomIndexRegistrationTests : TestBase
{
    private const string IndexAlias = "My_Index";

    private IndexOptions IndexOptions => GetRequiredService<IOptions<IndexOptions>>().Value;

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        builder.Services.Configure<IndexOptions>(options =>
        {
            options.RegisterIndex<IIndexer, ISearcher>(IndexAlias);
        });
    }

    [Test]
    public void Can_Get_Index_Registration()
    {
        IndexRegistration? indexRegistration = IndexOptions.GetIndexRegistration(IndexAlias);
        Assert.That(indexRegistration, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(indexRegistration.IndexAlias, Is.EqualTo(IndexAlias));
            Assert.That(indexRegistration.Indexer, Is.EqualTo(typeof(IIndexer)));
            Assert.That(indexRegistration.Searcher, Is.EqualTo(typeof(ISearcher)));
        });
    }

    [Test]
    public void Can_Resolve_Required_Searcher()
    {
        ISearcherResolver searcherResolver = GetRequiredService<ISearcherResolver>();
        Assert.That(searcherResolver.GetRequiredSearcher(IndexAlias), Is.TypeOf<TestIndexerAndSearcher>());
    }

    [Test]
    public void Can_Resolve_Required_Indexer()
    {
        IIndexerResolver indexerResolver = GetRequiredService<IIndexerResolver>();
        Assert.That(indexerResolver.GetRequiredIndexer(IndexAlias), Is.TypeOf<TestIndexerAndSearcher>());
    }

    [Test]
    public async Task Can_Populate_Custom_Index()
    {
        IIndexer indexer = GetRequiredService<IIndexer>();
        var id = Guid.NewGuid();
        await indexer.AddOrUpdateAsync(
            IndexAlias,
            id,
            UmbracoObjectTypes.Unknown,
            variations: [new Variation(null, null)],
            fields:
            [
                new IndexField("FieldOne", new() { Texts = ["This is field one"] }, null, null),
                new IndexField("FieldTwo", new() { Integers = [222] }, null, null),
            ],
            protection: null);

        ISearcher searcher = GetRequiredService<ISearcher>();
        SearchResult searchResult = await searcher.SearchAsync(IndexAlias, take: 10);

        Assert.Multiple(() =>
        {
            Assert.That(searchResult.Total, Is.EqualTo(1));
            Assert.That(searchResult.Documents.Count(), Is.EqualTo(1));
        });

        Assert.That(searchResult.Documents.First().Id, Is.EqualTo(id));
    }

    [Test]
    public void Cannot_Get_Index_Registration_As_Content_Index_Registration()
        => Assert.That(IndexOptions.GetContentIndexRegistration("My_Index"), Is.Null);
}
