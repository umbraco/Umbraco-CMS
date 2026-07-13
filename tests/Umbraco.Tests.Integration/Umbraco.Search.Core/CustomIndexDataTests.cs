using Umbraco.Cms.Core.Models;
using NUnit.Framework;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Cms.Search.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing.Search;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

// This test fixture is here to ensure that we don't accidentally make it too cumbersome to index custom data.
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
public class CustomIndexDataTests : TestBase
{
    [Test]
    public async Task CanIndexCustomData()
    {
        IIndexer indexer = GetRequiredService<IIndexer>();
        var key = Guid.NewGuid();
        await indexer.AddOrUpdateAsync(
            "My_Data",
            key,
            UmbracoObjectTypes.Unknown,
            [new Variation(Culture: null, Segment: null)],
            [
                new IndexField(
                    "FieldOne",
                    new IndexValue
                    {
                        Decimals = [12.34m],
                        Integers = [1234],
                        Keywords = ["one"],
                        Texts = ["some text"]
                    },
                    Culture: null,
                    Segment: null)
            ],
            protection: null);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump("My_Data");

        Assert.That(documents.Count, Is.EqualTo(1));
        Assert.Multiple(() =>
        {
            TestIndexDocument document = documents[0];
            Assert.That(document.Id, Is.EqualTo(key));
            Assert.That(document.ObjectType, Is.EqualTo(UmbracoObjectTypes.Unknown));

            Assert.That(document.Variations.Single().Culture, Is.Null);
            Assert.That(document.Variations.Single().Segment, Is.Null);

            IndexValue indexValue = document.Fields.Single().Value;
            Assert.That(indexValue.Decimals?.Single(), Is.EqualTo(12.34m));
            Assert.That(indexValue.Integers?.Single(), Is.EqualTo(1234));
            Assert.That(indexValue.Keywords?.Single(), Is.EqualTo("one"));
            Assert.That(indexValue.Texts?.Single(), Is.EqualTo("some text"));
        });
    }
}
