using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence;

// The connect timeout fallback is obsolete but still supported, so it still needs covering.
#pragma warning disable CS0618 // Type or member is obsolete

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
        => Assert.That(CommandTimeoutResolver.GetConfiguredCommandTimeout(null, "Server=.;Database=x"), Is.Null);

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Cannot_Get_Configured_Command_Timeout_Without_Connection_String(string connectionString)
        => Assert.That(CommandTimeoutResolver.GetConfiguredCommandTimeout(SqlServer, connectionString), Is.Null);

    [Test]
    [TestCase("Server=.;Database=x;Connect Timeout=60", "connect timeout", 60)]
    [TestCase("Server=.;Database=x;Connection Timeout=60", "connection timeout", 60)]
    public void Can_Apply_Connect_Timeout_As_Command_Timeout(
        string connectionString,
        string expectedKeyword,
        int expectedTimeout)
    {
        var result = CommandTimeoutResolver.TryGetDeprecatedConnectTimeout(
            SqlServer, connectionString, out var timeout, out var keyword);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(timeout, Is.EqualTo(expectedTimeout));
            Assert.That(keyword, Is.EqualTo(expectedKeyword));
        });
    }

    [Test]
    public void Can_Apply_Connect_Timeout_That_Shortens_Command_Timeout()
    {
        // The fallback is preserved exactly as it behaved before, in both directions: a connect timeout
        // shorter than the provider default has always shortened commands too.
        var result = CommandTimeoutResolver.TryGetDeprecatedConnectTimeout(
            SqlServer, "Server=.;Database=x;Connect Timeout=15", out var timeout, out _);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(timeout, Is.EqualTo(15));
        });
    }

    [Test]
    [TestCase("Server=.;Database=x", TestName = "no timeout keyword")]
    [TestCase("Server=.;Database=x;Timeout=45", TestName = "the Timeout synonym is deliberately not honoured")]
    [TestCase("Server=.;Database=x;Connect Timeout=abc", TestName = "unparseable connect timeout")]
    [TestCase("Server=.;Database=x;Connect Timeout=0", TestName = "connect timeout of no limit")]
    [TestCase("Server=.;Database=x;Connect Timeout=60;Command Timeout=300", TestName = "command timeout wins")]
    [TestCase("Server=.;Database=x;Connect Timeout=60;Command Timeout=0", TestName = "command timeout of no limit wins")]
    public void Cannot_Apply_Connect_Timeout_As_Command_Timeout(string connectionString)
    {
        var result = CommandTimeoutResolver.TryGetDeprecatedConnectTimeout(
            SqlServer, connectionString, out var timeout, out var keyword);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            Assert.That(timeout, Is.EqualTo(0));
            Assert.That(keyword, Is.Null);
        });
    }

    [Test]
    public void Cannot_Apply_Connect_Timeout_To_Sqlite_Connection_String()
    {
        // SQLite has no connect timeout, so the keyword can never legitimately appear.
        var result = CommandTimeoutResolver.TryGetDeprecatedConnectTimeout(
            Sqlite, "Data Source=x.db;Default Timeout=600", out _, out _);

        Assert.That(result, Is.False);
    }
}
#pragma warning restore CS0618 // Type or member is obsolete
