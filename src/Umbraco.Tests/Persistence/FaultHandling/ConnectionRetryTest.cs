using System;
using System.Data.SqlClient;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Scoping;

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
            var factory = new UmbracoDatabaseFactory(connectionString, providerName, Mock.Of<ILogger>(), new Lazy<IMapperCollection>(() => Mock.Of<IMapperCollection>()));

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
            var factory = new UmbracoDatabaseFactory(connectionString, providerName, Mock.Of<ILogger>(), new Lazy<IMapperCollection>(() => Mock.Of<IMapperCollection>()));

            using (var database = factory.CreateDatabase())
            {
                Assert.Throws<SqlException>(
                () => database.Fetch<dynamic>("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES"));
            }
        }
    }
}
