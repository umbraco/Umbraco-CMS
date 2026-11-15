using NUnit.Framework;
using Umbraco.Cms.Core.Search.Indexing;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Search.Core.Persistence;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class IndexDocumentRepositoryTests : UmbracoIntegrationTest
{
    // Exceeds the parameter ceiling of every supported database (SQL Server: 2100, SQLite: 32766).
    private const int IdCount = 40_000;

    [Test]
    public async Task DeleteAsync_Supports_More_Ids_Than_The_Database_Parameter_Limit()
    {
        var repository = new IndexDocumentRepository((IScopeAccessor)ScopeProvider);

        Guid deletedKey = Guid.NewGuid();
        Guid retainedKey = Guid.NewGuid();

        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            await repository.AddAsync(CreateDocument(deletedKey, published: true));
            await repository.AddAsync(CreateDocument(retainedKey, published: true));
            await repository.AddAsync(CreateDocument(deletedKey, published: false));

            Guid[] ids = Enumerable.Range(0, IdCount)
                .Select(_ => Guid.NewGuid())
                .Append(deletedKey)
                .ToArray();

            await repository.DeleteAsync(ids, published: true);

            Assert.Multiple(async () =>
            {
                Assert.That(await repository.GetAsync(deletedKey, published: true), Is.Null);
                Assert.That(await repository.GetAsync(retainedKey, published: true), Is.Not.Null);
                Assert.That(await repository.GetAsync(deletedKey, published: false), Is.Not.Null);
            });
        }
    }

    private static IndexDocument CreateDocument(Guid key, bool published) =>
        new() { Key = key, Published = published, Fields = [] };
}
