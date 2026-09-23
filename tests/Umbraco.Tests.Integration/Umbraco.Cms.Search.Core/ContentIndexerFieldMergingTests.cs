// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Search.Indexing;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.Testing.Search;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

public class ContentIndexerFieldMergingTests : ContentTestBase
{
    private const string SharedFieldName = "sharedField";

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        base.ConfigureTestServices(services);

        services.AddTransient<IContentIndexer, KeywordContributingIndexer>();
        services.AddTransient<IContentIndexer, OverlappingKeywordContributingIndexer>();
        services.AddTransient<IContentIndexer, TextContributingIndexer>();
    }

    [Test]
    public async Task MultipleIndexers_ContributingToTheSameField_AreMergedAndDeduplicated()
    {
        IContentType contentType = new ContentTypeBuilder()
            .WithAlias("noProperties")
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        Content content = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("My Content")
            .Build();
        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);

        AssertMergedField(IndexAliases.DraftContent);
        AssertMergedField(IndexAliases.PublishedContent);

        return;

        void AssertMergedField(string indexAlias)
        {
            IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(indexAlias);
            Assert.That(documents, Has.Count.EqualTo(1));

            TestIndexDocument document = documents.Single();
            IndexField[] sharedFields = document.Fields.Where(f => f.FieldName == SharedFieldName).ToArray();

            // all three contributions must have landed on the exact same field - not separate, colliding ones
            Assert.That(sharedFields, Has.Length.EqualTo(1));

            IndexValue value = sharedFields[0].Value;
            Assert.Multiple(() =>
            {
                // "shared-keyword" is contributed by two indexers and must only appear once
                CollectionAssert.AreEquivalent(new[] { "own-keyword", "shared-keyword" }, value.Keywords);

                // the third indexer's Texts contribution must survive alongside the merged keywords
                CollectionAssert.AreEquivalent(new[] { "the-value" }, value.Texts);
            });
        }
    }

    private sealed class KeywordContributingIndexer : IContentIndexer
    {
        public Task<IEnumerable<IndexField>> GetIndexFieldsAsync(IContentBase content, string?[] cultures, bool published, CancellationToken cancellationToken)
            => Task.FromResult<IEnumerable<IndexField>>(
            [
                new IndexField(SharedFieldName, new IndexValue { Keywords = ["shared-keyword"] }, null, null)
            ]);
    }

    private sealed class OverlappingKeywordContributingIndexer : IContentIndexer
    {
        public Task<IEnumerable<IndexField>> GetIndexFieldsAsync(IContentBase content, string?[] cultures, bool published, CancellationToken cancellationToken)
            => Task.FromResult<IEnumerable<IndexField>>(
            [
                new IndexField(SharedFieldName, new IndexValue { Keywords = ["shared-keyword", "own-keyword"] }, null, null)
            ]);
    }

    private sealed class TextContributingIndexer : IContentIndexer
    {
        public Task<IEnumerable<IndexField>> GetIndexFieldsAsync(IContentBase content, string?[] cultures, bool published, CancellationToken cancellationToken)
            => Task.FromResult<IEnumerable<IndexField>>(
            [
                new IndexField(SharedFieldName, new IndexValue { Texts = ["the-value"] }, null, null)
            ]);
    }
}
