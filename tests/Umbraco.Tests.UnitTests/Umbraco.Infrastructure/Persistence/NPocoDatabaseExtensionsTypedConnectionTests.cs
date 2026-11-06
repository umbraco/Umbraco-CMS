using System.Data.Common;
using Microsoft.Data.Sqlite;
using NUnit.Framework;
using StackExchange.Profiling.Data;
using Umbraco.Cms.Infrastructure.Persistence.FaultHandling;
using Umbraco.Cms.Persistence.Sqlite.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence;

[TestFixture]
public class NPocoDatabaseExtensionsTypedConnectionTests
{
    [Test]
    public void Can_Unwrap_Typed_Connection_Through_All_Decorators()
    {
        using var sqliteConnection = new SqliteConnection("Data Source=:memory:");
        DbConnection wrapped = new ProfiledDbConnection(
            new RetryDbConnection(new SqlitePreferDeferredTransactionsConnection(sqliteConnection), null, null),
            null);

        Assert.Multiple(() =>
        {
            Assert.That(NPocoDatabaseExtensions.TryGetTypedConnection(wrapped, out SqliteConnection? unwrapped), Is.True);
            Assert.That(unwrapped, Is.SameAs(sqliteConnection));
            Assert.That(NPocoDatabaseExtensions.GetTypedConnection<SqliteConnection>(wrapped), Is.SameAs(sqliteConnection));
        });
    }

    [Test]
    public void Cannot_Unwrap_Typed_Connection_When_Not_Present()
    {
        using var sqliteConnection = new SqliteConnection("Data Source=:memory:");
        DbConnection wrapped = new ProfiledDbConnection(sqliteConnection, null);

        Assert.Multiple(() =>
        {
            Assert.That(NPocoDatabaseExtensions.TryGetTypedConnection(wrapped, out Microsoft.Data.SqlClient.SqlConnection? unwrapped), Is.False);
            Assert.That(unwrapped, Is.Null);
            Assert.Throws<NotSupportedException>(() => NPocoDatabaseExtensions.GetTypedConnection<Microsoft.Data.SqlClient.SqlConnection>(wrapped));
        });
    }
}
