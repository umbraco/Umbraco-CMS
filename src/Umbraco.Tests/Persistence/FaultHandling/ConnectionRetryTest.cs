using System.Data.SqlClient;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;

namespace Umbraco.Tests.Persistence.FaultHandling
{
    [TestFixture, NUnit.Framework.Ignore]
    public class ConnectionRetryTest
    {
        [Test]
        public void PetaPocoConnection_Cant_Connect_To_SqlDatabase_With_Invalid_User()
        {
            // Arrange
            const string providerName = "System.Data.SqlClient";
            const string connectionString = @"server=.\SQLEXPRESS;database=EmptyForTest;user id=x;password=umbraco";
            var factory = new DefaultDatabaseFactory(connectionString, providerName, Mock.Of<ILogger>());
            var database = factory.CreateDatabase();

            //Act
            Assert.Throws<SqlException>(
                () => database.Fetch<dynamic>("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES"));
        }

        [Test]
        public void PetaPocoConnection_Cant_Connect_To_SqlDatabase_Because_Of_Network()
        {
            // Arrange
            const string providerName = "System.Data.SqlClient";
            const string connectionString = @"server=.\SQLEXPRESS;database=EmptyForTest;user id=umbraco;password=umbraco";
            var factory = new DefaultDatabaseFactory(connectionString, providerName, Mock.Of<ILogger>());
            var database = factory.CreateDatabase();

            //Act
            Assert.Throws<SqlException>(
                () => database.Fetch<dynamic>("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES"));
        }
    }
}