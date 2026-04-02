// Copyright (c) Umbraco.
// See LICENSE for more details.

using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.NPocoTests;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class NPocoUpdateBatchTests : UmbracoIntegrationTest
{
    [Test]
    public void Can_Update_Batch()
    {
        // Arrange
        var servers = new List<ServerRegistrationDto>();
        for (var i = 0; i < 3; i++)
        {
            servers.Add(new ServerRegistrationDto
            {
                Id = i + 1,
                ServerAddress = "address" + i,
                ServerIdentity = "computer" + i,
                DateRegistered = DateTime.Now,
                IsActive = false,
                DateAccessed = DateTime.Now,
            });
        }

        using (var scope = ScopeProvider.CreateScope())
        {
            scope.Database.BulkInsertRecords(servers);
            scope.Complete();
        }

        // Act
        for (var i = 0; i < 3; i++)
        {
            servers[i].ServerAddress = "newaddress" + i;
            servers[i].IsActive = true;
        }

        using (var scope = ScopeProvider.CreateScope())
        {
            var updateBatch = servers
                .Select(x => UpdateBatch.For(x))
                .ToList();
            var updated = scope.Database.UpdateBatch(updateBatch, new BatchOptions { BatchSize = 100 });
            Assert.AreEqual(3, updated);
            scope.Complete();
        }

        // Assert
        using (var scope = ScopeProvider.CreateScope())
        {
            var dtos = scope.Database.Fetch<ServerRegistrationDto>();
            Assert.AreEqual(3, dtos.Count);
            for (var i = 0; i < 3; i++)
            {
                Assert.AreEqual(servers[i].ServerAddress, dtos[i].ServerAddress);
                Assert.AreEqual(servers[i].ServerIdentity, dtos[i].ServerIdentity);
                Assert.AreEqual(servers[i].IsActive, dtos[i].IsActive);
            }
        }
    }
}
