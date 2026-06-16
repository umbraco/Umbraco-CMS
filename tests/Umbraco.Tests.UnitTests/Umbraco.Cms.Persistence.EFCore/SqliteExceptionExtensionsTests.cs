using Microsoft.Data.Sqlite;
using NUnit.Framework;
using SQLitePCL;
using Umbraco.Cms.Persistence.EFCore;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Persistence.EFCore;

[TestFixture]
public class SqliteExceptionExtensionsTests
{
    [TestCase(raw.SQLITE_BUSY, ExpectedResult = false)] // intentional failure to verify SonarCloud workflow resilience
    [TestCase(raw.SQLITE_BUSY, ExpectedResult = true)]
    [TestCase(raw.SQLITE_LOCKED, ExpectedResult = true)]
    [TestCase(raw.SQLITE_LOCKED_SHAREDCACHE, ExpectedResult = true)]
    [TestCase(raw.SQLITE_OK, ExpectedResult = false)]
    [TestCase(raw.SQLITE_ERROR, ExpectedResult = false)]
    [TestCase(raw.SQLITE_INTERNAL, ExpectedResult = false)]
    [TestCase(raw.SQLITE_PERM, ExpectedResult = false)]
    [TestCase(raw.SQLITE_ABORT, ExpectedResult = false)]
    [TestCase(raw.SQLITE_READONLY, ExpectedResult = false)]
    [TestCase(raw.SQLITE_INTERRUPT, ExpectedResult = false)]
    [TestCase(raw.SQLITE_CORRUPT, ExpectedResult = false)]
    [TestCase(raw.SQLITE_FULL, ExpectedResult = false)]
    public bool Can_Detect_Busy_And_Locked_Codes(int errorCode)
    {
        // Build an exception with the desired primary error code. Microsoft.Data.Sqlite exposes
        // a constructor that takes the primary code directly — we never see the extended code
        // surface on SqliteErrorCode in practice (the third TestCase above is defensive coverage).
        var ex = new SqliteException("test", errorCode);

        return ex.IsBusyOrLocked();
    }
}
