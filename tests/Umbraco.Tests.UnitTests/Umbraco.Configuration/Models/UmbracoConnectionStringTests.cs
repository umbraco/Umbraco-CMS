// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Configuration.Models
{
    public class UmbracoConnectionStringTests
    {
        [Test]
        [TestCase("", ExpectedResult = null)]
        [TestCase(null, ExpectedResult = null)]
        [TestCase(@"Data Source=|DataDirectory|\Umbraco.sdf;Flush Interval=1;", ExpectedResult = Constants.DbProviderNames.SqlCe)]
        [TestCase(@"Server=(LocalDb)\Umbraco;Database=NetCore;Integrated Security=true", ExpectedResult = Constants.DbProviderNames.SqlServer)]
        [TestCase(@"Data Source=(LocalDb)\Umbraco;Initial Catalog=NetCore;Integrated Security=true;", ExpectedResult = Constants.DbProviderNames.SqlServer)]
        [TestCase(@"Data Source=.\SQLExpress;Integrated Security=true;AttachDbFilename=MyDataFile.mdf;", ExpectedResult = Constants.DbProviderNames.SqlServer)]
        public string ParseProviderName(string connectionString)
            => UmbracoConnectionString.ParseProviderName(connectionString);
    }
}
