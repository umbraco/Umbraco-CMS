// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Data.Common;
using System.Linq;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class ServerRegistrationRepositoryTest : UmbracoIntegrationTest
{
    [SetUp]
    public void SetUp()
    {
        _appCaches = AppCaches.Disabled;
        CreateTestData();
    }

    private AppCaches _appCaches;

    private ServerRegistrationRepository CreateRepository(IScopeProvider provider) =>
        new((IScopeAccessor)provider, LoggerFactory.CreateLogger<ServerRegistrationRepository>());

    [Test]
    public void Cannot_Add_Duplicate_Server_Identities()
    {
        // Arrange
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            var server = new ServerRegistration("http://shazwazza.com", "COMPUTER1", DateTime.Now);

            Assert.That(() => repository.Save(server), Throws.InstanceOf<DbException>());
        }
    }

    [Test]
    public void Cannot_Update_To_Duplicate_Server_Identities()
    {
        // Arrange
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            var server = repository.Get(1);
            server.ServerIdentity = "COMPUTER2";

            Assert.That(() => repository.Save(server), Throws.InstanceOf<DbException>());
        }
    }

    [Test]
    public void Can_Instantiate_Repository()
    {
        // Arrange
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            // Assert
            Assert.That(repository, Is.Not.Null);
        }
    }

    [Test]
    public void Can_Perform_Get_On_Repository()
    {
        // Arrange
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            // Act
            var server = repository.Get(1);

            // Assert
            Assert.That(server, Is.Not.Null);
            Assert.That(server.HasIdentity, Is.True);
            Assert.That(server.ServerAddress, Is.EqualTo("http://localhost"));
        }
    }

    [Test]
    public void Can_Perform_GetAll_On_Repository()
    {
        // Arrange
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            // Act
            var servers = repository.GetMany();

            // Assert
            Assert.That(servers.Count(), Is.EqualTo(3));
        }
    }

    // queries are not supported due to in-memory caching
    //// [Test]
    //// public void Can_Perform_GetByQuery_On_Repository()
    //// {
    ////    // Arrange
    ////    var provider = ScopeProvider;
    ////    using (var unitOfWork = provider.GetUnitOfWork())
    ////    using (var repository = CreateRepository(provider))
    ////    {
    ////        // Act
    ////        var query = Query<IServerRegistration>.Builder.Where(x => x.ServerIdentity.ToUpper() == "COMPUTER3");
    ////        var result = repository.GetByQuery(query);

    //// // Assert
    ////        Assert.AreEqual(1, result.Count());
    ////    }
    //// }

    //// [Test]
    //// public void Can_Perform_Count_On_Repository()
    //// {
    ////    // Arrange
    ////    var provider = ScopeProvider;
    ////    using (var unitOfWork = provider.GetUnitOfWork())
    ////    using (var repository = CreateRepository(provider))
    ////    {
    ////        // Act
    ////        var query = Query<IServerRegistration>.Builder.Where(x => x.ServerAddress.StartsWith("http://"));
    ////        int count = repository.Count(query);

    //// // Assert
    ////        Assert.That(count, Is.EqualTo(2));
    ////    }
    //// }

    [Test]
    public void Can_Perform_Add_On_Repository()
    {
        // Arrange
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            // Act
            var server = new ServerRegistration("http://shazwazza.com", "COMPUTER4", DateTime.Now);
            repository.Save(server);

            // Assert
            Assert.That(server.HasIdentity, Is.True);
            Assert.That(server.Id, Is.EqualTo(4)); // With 3 existing entries the Id should be 4
        }
    }

    [Test]
    public void Can_Perform_Update_On_Repository()
    {
        // Arrange
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            // Act
            var server = repository.Get(2);
            server.ServerAddress = "https://umbraco.com";
            server.IsActive = true;

            repository.Save(server);

            var serverUpdated = repository.Get(2);

            // Assert
            Assert.That(serverUpdated, Is.Not.Null);
            Assert.That(serverUpdated.ServerAddress, Is.EqualTo("https://umbraco.com"));
            Assert.That(serverUpdated.IsActive, Is.EqualTo(true));
        }
    }

    [Test]
    public void Can_Perform_Delete_On_Repository()
    {
        // Arrange
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            // Act
            var server = repository.Get(3);
            Assert.IsNotNull(server);
            repository.Delete(server);

            var exists = repository.Exists(3);

            // Assert
            Assert.That(exists, Is.False);
        }
    }

    [Test]
    public void Can_Perform_Exists_On_Repository()
    {
        // Arrange
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            // Act
            var exists = repository.Exists(3);
            var doesntExist = repository.Exists(10);

            // Assert
            Assert.That(exists, Is.True);
            Assert.That(doesntExist, Is.False);
        }
    }

    public void CreateTestData()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            repository.Save(new ServerRegistration("http://localhost", "COMPUTER1", DateTime.Now) { IsActive = true });
            repository.Save(new ServerRegistration("http://www.mydomain.com", "COMPUTER2", DateTime.Now));
            repository.Save(new ServerRegistration("https://www.another.domain.com", "Computer3", DateTime.Now));
            scope.Complete();
        }
    }
}
