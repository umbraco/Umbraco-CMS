﻿using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;

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
            var connectionStrings = new ConnectionStrings
            {
                UmbracoConnectionString = new ConfigConnectionString(Constants.System.UmbracoConnectionName, connectionString)
            };

            var actual = connectionStrings.UmbracoConnectionString;

            Assert.AreEqual(connectionString, actual.ConnectionString);
            Assert.AreEqual(Constants.System.UmbracoConnectionName, actual.Name);

            return connectionStrings.UmbracoConnectionString.ProviderName;
        }
    }
}
