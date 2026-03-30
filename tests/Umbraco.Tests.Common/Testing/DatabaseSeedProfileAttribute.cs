// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Tests.Common.Testing;

/// <summary>
///     Opt-in attribute that selects an <see cref="ITestDatabaseSeedProfile"/> for the fixture.
///     When present, the test base class uses snapshot-aware database swapping with the specified
///     seed profile instead of a fresh database per fixture.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public class DatabaseSeedProfileAttribute : Attribute
{
    public DatabaseSeedProfileAttribute(Type seedProfileType)
    {
        if (!typeof(ITestDatabaseSeedProfile).IsAssignableFrom(seedProfileType))
        {
            throw new ArgumentException(
                $"{seedProfileType.Name} must implement {nameof(ITestDatabaseSeedProfile)}.",
                nameof(seedProfileType));
        }

        SeedProfileType = seedProfileType;
    }

    /// <summary>
    ///     The <see cref="ITestDatabaseSeedProfile"/> type to instantiate and use for seeding.
    /// </summary>
    public Type SeedProfileType { get; }
}
