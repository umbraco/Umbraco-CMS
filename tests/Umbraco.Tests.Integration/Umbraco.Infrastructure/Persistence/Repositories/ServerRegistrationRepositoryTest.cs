// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class ServerRegistrationRepositoryTest : UmbracoIntegrationTest
    {
        private AppCaches _appCaches;

        [SetUp]
        public void SetUp()
        {
            _appCaches = AppCaches.Disabled;
            CreateTestData();
        }

        private ServerRegistrationRepository CreateRepository(IScopeProvider provider) =>
            new ServerRegistrationRepository((IScopeAccessor)provider, LoggerFactory.CreateLogger<ServerRegistrationRepository>());

        [Test]
        public void Cannot_Add_Duplicate_Server_Identities()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                ServerRegistrationRepository repository = CreateRepository(provider);

                var server = new ServerRegistration("http://shazwazza.com", "COMPUTER1", DateTime.Now);

                Assert.That(() => repository.Save(server), Throws.InstanceOf<DbException>());

                scope.Rollback();
            }
        }

        [Test]
        public void Cannot_Update_To_Duplicate_Server_Identities()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                ServerRegistrationRepository repository = CreateRepository(provider);

                IServerRegistration server = repository.Get(1);
                server.ServerIdentity = "COMPUTER2";

                Assert.That(() => repository.Save(server), Throws.InstanceOf<DbException>());

                scope.Rollback();
            }
        }

        [Test]
        public void Can_Instantiate_Repository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope())
            {
                ServerRegistrationRepository repository = CreateRepository(provider);

                // Assert
                Assert.That(repository, Is.Not.Null);
            }
        }

        [Test]
        public void Can_Perform_Get_On_Repository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope(autoComplete: true))
            {
                ServerRegistrationRepository repository = CreateRepository(provider);

                // Act
                IServerRegistration server = repository.Get(1);

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
            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope(autoComplete: true))
            {
                ServerRegistrationRepository repository = CreateRepository(provider);

                // Act
                IEnumerable<IServerRegistration> servers = repository.GetMany();

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
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                ServerRegistrationRepository repository = CreateRepository(provider);

                // Act
                var server = new ServerRegistration("http://shazwazza.com", "COMPUTER4", DateTime.Now);
                repository.Save(server);

                scope.Rollback();

                // Assert
                Assert.That(server.HasIdentity, Is.True);
                Assert.That(server.Id, Is.EqualTo(4)); // With 3 existing entries the Id should be 4
            }
        }

        [Test]
        public void Can_Perform_Update_On_Repository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                ServerRegistrationRepository repository = CreateRepository(provider);

                // Act
                IServerRegistration server = repository.Get(2);
                server.ServerAddress = "https://umbraco.com";
                server.IsActive = true;

                repository.Save(server);

                IServerRegistration serverUpdated = repository.Get(2);

                scope.Rollback();

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
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                ServerRegistrationRepository repository = CreateRepository(provider);

                // Act
                IServerRegistration server = repository.Get(3);
                Assert.IsNotNull(server);
                repository.Delete(server);

                bool exists = repository.Exists(3);

                scope.Rollback();

                // Assert
                Assert.That(exists, Is.False);
            }
        }

        [Test]
        public void Can_Perform_Exists_On_Repository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope(autoComplete: true))
            {
                ServerRegistrationRepository repository = CreateRepository(provider);

                // Act
                bool exists = repository.Exists(3);
                bool doesntExist = repository.Exists(10);

                // Assert
                Assert.That(exists, Is.True);
                Assert.That(doesntExist, Is.False);
            }
        }

        public void CreateTestData()
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                ServerRegistrationRepository repository = CreateRepository(provider);

                repository.Save(new ServerRegistration("http://localhost", "COMPUTER1", DateTime.Now) { IsActive = true });
                repository.Save(new ServerRegistration("http://www.mydomain.com", "COMPUTER2", DateTime.Now));
                repository.Save(new ServerRegistration("https://www.another.domain.com", "Computer3", DateTime.Now));
                scope.Complete();
            }
        }
    }
}
