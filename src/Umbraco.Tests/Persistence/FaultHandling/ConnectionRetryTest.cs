using System;
using System.Data.SqlClient;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence.FaultHandling
{
    [TestFixture, NUnit.Framework.Ignore]
    public class ConnectionRetryTest
    {
        [Test]
        public void Cant_Connect_To_SqlDatabase_With_Invalid_User()
        {
            // Arrange
            const string connectionString = @"server=.\SQLEXPRESS;database=EmptyForTest;user id=x;password=umbraco";
            const string providerName = Constants.DbProviderNames.SqlServer;
            var sqlSyntax = new[] { new SqlServerSyntaxProvider(new Lazy<IDatabaseFactory>(() => null)) };
            var factory = new DefaultDatabaseFactory(connectionString, providerName, sqlSyntax, Mock.Of<ILogger>(), new TestScopeContextAdapter(), Mock.Of<IMappingResolver>());
            var database = factory.GetDatabase();

            //Act
            Assert.Throws<SqlException>(
                () => database.Fetch<dynamic>("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES"));
        }

        [Test]
        public void Cant_Connect_To_SqlDatabase_Because_Of_Network()
        {
            // Arrange
            const string connectionString = @"server=.\SQLEXPRESS;database=EmptyForTest;user id=umbraco;password=umbraco";
            const string providerName = Constants.DbProviderNames.SqlServer;
            var sqlSyntax = new[] { new SqlServerSyntaxProvider(new Lazy<IDatabaseFactory>(() => null)) };
            var factory = new DefaultDatabaseFactory(connectionString, providerName, sqlSyntax, Mock.Of<ILogger>(), new TestScopeContextAdapter(), Mock.Of<IMappingResolver>());
            var database = factory.GetDatabase();

            //Act
            Assert.Throws<SqlException>(
                () => database.Fetch<dynamic>("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES"));
        }
    }
}