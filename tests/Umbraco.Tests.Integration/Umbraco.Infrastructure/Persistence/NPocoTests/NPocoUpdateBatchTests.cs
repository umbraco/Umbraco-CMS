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
                ServerAddress = "address" + i,
                ServerIdentity = "computer" + i,
                DateRegistered = DateTime.Now,
                IsActive = false,
                DateAccessed = DateTime.Now,
            });
        }

        using (var scope = ScopeProvider.CreateScope())
        {
            ScopeAccessor.AmbientScope.Database.BulkInsertRecords(servers);
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
            ScopeAccessor.AmbientScope.Database.UpdateBatch(updateBatch, new BatchOptions { BatchSize = 100 });
            scope.Complete();
        }

        // Assert
        using (var scope = ScopeProvider.CreateScope())
        {
            var dtos = ScopeAccessor.AmbientScope.Database.Fetch<ServerRegistrationDto>();
            Assert.AreEqual(3, dtos.Count);
            for (var i = 0; i < 3; i++)
            {
                dtos[i].ServerAddress = servers[i].ServerAddress;
                dtos[i].ServerIdentity = servers[i].ServerIdentity;
                dtos[i].IsActive = servers[i].IsActive;
            }
        }
    }
}
