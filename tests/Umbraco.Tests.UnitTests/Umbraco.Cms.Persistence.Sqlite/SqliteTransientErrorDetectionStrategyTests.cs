using Microsoft.Data.Sqlite;
using NUnit.Framework;
using SQLitePCL;
using Umbraco.Cms.Persistence.Sqlite.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Persistence.Sqlite;

[TestFixture]
public class SqliteTransientErrorDetectionStrategyTests
{
    private readonly SqliteTransientErrorDetectionStrategy _sut = new();

    [TestCase(raw.SQLITE_BUSY, raw.SQLITE_BUSY)]
    [TestCase(raw.SQLITE_LOCKED, raw.SQLITE_LOCKED)]
    [TestCase(raw.SQLITE_LOCKED, raw.SQLITE_LOCKED_SHAREDCACHE)]
    public void Busy_Or_Locked_Is_Transient(int errorCode, int extendedErrorCode)
        => Assert.IsTrue(_sut.IsTransient(new SqliteException("error", errorCode, extendedErrorCode)));

    [Test]
    public void Busy_Snapshot_Is_Not_Transient()
        => Assert.IsFalse(_sut.IsTransient(new SqliteException("error", raw.SQLITE_BUSY, raw.SQLITE_BUSY_SNAPSHOT)));

    [Test]
    public void Constraint_Violation_Is_Not_Transient()
        => Assert.IsFalse(_sut.IsTransient(new SqliteException("error", raw.SQLITE_CONSTRAINT, raw.SQLITE_CONSTRAINT_UNIQUE)));

    [Test]
    public void Non_Sqlite_Exception_Is_Not_Transient()
        => Assert.IsFalse(_sut.IsTransient(new InvalidOperationException()));
}
