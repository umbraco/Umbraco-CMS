using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence;

[TestFixture]
public class CommandTimeoutResolverTests
{
    private static DbProviderFactory SqlServer => SqlClientFactory.Instance;

    private static DbProviderFactory Sqlite => SqliteFactory.Instance;

    [Test]
    [TestCase("Server=.;Database=x", ExpectedResult = 30)]
    [TestCase("Server=.;Database=x;Command Timeout=300", ExpectedResult = 300)]
    [TestCase("Server=.;Database=x;Command Timeout=0", ExpectedResult = 0)]
    [TestCase("Server=.;Database=x;Connect Timeout=600", ExpectedResult = 30)]
    [TestCase("Server=.;Database=x;Timeout=600", ExpectedResult = 30)]
    public int? Can_Get_Configured_Command_Timeout_For_Sql_Server(string connectionString)
        => CommandTimeoutResolver.GetConfiguredCommandTimeout(SqlServer, connectionString);

    [Test]
    [TestCase("Data Source=x.db", ExpectedResult = 30)]
    [TestCase("Data Source=x.db;Default Timeout=600", ExpectedResult = 600)]
    [TestCase("Data Source=x.db;Command Timeout=600", ExpectedResult = 600)]
    [TestCase("Data Source=x.db;Connection Timeout=30", ExpectedResult = null)]
    public int? Can_Get_Configured_Command_Timeout_For_Sqlite(string connectionString)
        => CommandTimeoutResolver.GetConfiguredCommandTimeout(Sqlite, connectionString);

    [Test]
    public void Cannot_Get_Configured_Command_Timeout_Without_Provider()
        => Assert.IsNull(CommandTimeoutResolver.GetConfiguredCommandTimeout(null, "Server=.;Database=x"));

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Cannot_Get_Configured_Command_Timeout_Without_Connection_String(string connectionString)
        => Assert.IsNull(CommandTimeoutResolver.GetConfiguredCommandTimeout(SqlServer, connectionString));
}
