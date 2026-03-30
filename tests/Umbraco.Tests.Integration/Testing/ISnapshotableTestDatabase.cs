// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Tests.Integration.Testing;

/// <summary>
///     Extends <see cref="ITestDatabase"/> with the ability to create and restore
///     named snapshots. This enables seeding a database once and restoring it
///     cheaply for subsequent test fixtures that need the same seed data.
/// </summary>
public interface ISnapshotableTestDatabase : ITestDatabase
{
    /// <summary>
    ///     Returns <c>true</c> if a snapshot with the given key has been created.
    /// </summary>
    bool HasSnapshot(string snapshotKey);

    /// <summary>
    ///     Creates a named snapshot from the given database.
    ///     The database should already have schema and seed data applied.
    /// </summary>
    void CreateSnapshot(string snapshotKey, TestDbMeta sourceMeta);

    /// <summary>
    ///     Attaches a new database restored from the named snapshot.
    /// </summary>
    TestDbMeta AttachFromSnapshot(string snapshotKey);
}
