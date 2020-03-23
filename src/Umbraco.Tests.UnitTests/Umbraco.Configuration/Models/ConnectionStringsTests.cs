using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using Umbraco.Configuration.Models;
using Umbraco.Core;

namespace Umbraco.Tests.UnitTests.Umbraco.Configuration.Models
{
    public class ConnectionStringsTests
    {
        [Test]
        [TestCase("", ExpectedResult = null)]
        [TestCase(null, ExpectedResult = null)]
        [TestCase(@"Data Source=|DataDirectory|\Umbraco.sdf;Flush Interval=1;", ExpectedResult = Constants.DbProviderNames.SqlCe)]
        [TestCase(@"Server=(LocalDb)\Umbraco;Database=NetCore;Integrated Security=true", ExpectedResult = Constants.DbProviderNames.SqlServer)]
        public string ParseProviderName(string connectionString)
        {
            var key = Constants.System.UmbracoConnectionName;
            var configuration = new Mock<IConfiguration>();


            //This is the underlying method that is called by Configuration.GetConnectionString(string)
            if (connectionString != null)
            {
                configuration.Setup(x => x.GetSection("ConnectionStrings")[key]).Returns(connectionString);
            }


            var connectionStrings = new ConnectionStrings(configuration.Object);

            var actual = connectionStrings[key];

            Assert.AreEqual(connectionString, actual.ConnectionString);
            Assert.AreEqual(key, actual.Name);

            return connectionStrings[key].ProviderName;
        }
    }
}
