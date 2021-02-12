using System;
using System.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Core;
using Umbraco.Core.Scoping;
using Umbraco.Tests.TestHelpers;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Tests.Persistence.FaultHandling
{
    [TestFixture, Ignore("fixme - ignored test")]
    public class ConnectionRetryTest
    {
        [Test]
        public void Cant_Connect_To_SqlDatabase_With_Invalid_User()
        {
            const string connectionString = @"server=.\SQLEXPRESS;database=EmptyForTest;user id=x;password=umbraco";
            const string providerName = Constants.DbProviderNames.SqlServer;
            var factory = new UmbracoDatabaseFactory(Mock.Of<ILogger<UmbracoDatabaseFactory>>(), NullLoggerFactory.Instance, connectionString, providerName, new Lazy<IMapperCollection>(() => Mock.Of<IMapperCollection>()), TestHelper.DbProviderFactoryCreator, TestHelper.DatabaseSchemaCreatorFactory);

            using (var database = factory.CreateDatabase())
            {
                Assert.Throws<SqlException>(
                    () => database.Fetch<dynamic>("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES"));
            }
        }

        [Test]
        public void Cant_Connect_To_SqlDatabase_Because_Of_Network()
        {
            const string connectionString = @"server=.\SQLEXPRESS;database=EmptyForTest;user id=umbraco;password=umbraco";
            const string providerName = Constants.DbProviderNames.SqlServer;
            var factory = new UmbracoDatabaseFactory(Mock.Of<ILogger<UmbracoDatabaseFactory>>(), NullLoggerFactory.Instance, connectionString, providerName, new Lazy<IMapperCollection>(() => Mock.Of<IMapperCollection>()), TestHelper.DbProviderFactoryCreator, TestHelper.DatabaseSchemaCreatorFactory);

            using (var database = factory.CreateDatabase())
            {
                Assert.Throws<SqlException>(
                () => database.Fetch<dynamic>("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES"));
            }
        }
    }
}
