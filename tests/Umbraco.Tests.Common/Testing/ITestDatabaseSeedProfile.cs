// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Tests.Common.Testing;

/// <summary>
///     Defines a named seed profile for test databases. Fixtures that return the same
///     <see cref="SeedKey"/> share the same snapshot: the first fixture seeds the database
///     and creates the snapshot, subsequent fixtures restore from the snapshot instantly.
/// </summary>
public interface ITestDatabaseSeedProfile
{
    /// <summary>
    ///     Unique key identifying this seed profile. Fixtures returning the same key
    ///     share the same database snapshot.
    /// </summary>
    string SeedKey { get; }

    /// <summary>
    ///     Seeds the database. Called once per unique <see cref="SeedKey"/>, then snapshotted.
    ///     The <paramref name="services"/> provider comes from the running host, so all
    ///     Umbraco services are available for creating content types, content, users, etc.
    /// </summary>
    Task SeedAsync(IServiceProvider services);
}
