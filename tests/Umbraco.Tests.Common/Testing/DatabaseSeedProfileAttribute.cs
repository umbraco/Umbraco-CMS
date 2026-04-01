namespace Umbraco.Cms.Tests.Common.Testing;

/// <summary>
///     Opt-in attribute that selects an <see cref="ITestDatabaseSeedProfile"/> for the fixture.
///     When present, the test base class uses snapshot-aware database swapping with the specified
///     seed profile instead of a fresh database per fixture.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public class DatabaseSeedProfileAttribute : Attribute
{
    /// <summary>
    /// Creates a new instance of <see cref="DatabaseSeedProfileAttribute"/>.
    /// </summary>
    /// <param name="seedProfileType">The Seedprofile to use.</param>
    /// <exception cref="ArgumentException">Thrown when the profile does not implement <see cref="ITestDatabaseSeedProfile"/>.</exception>
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
    ///     Gets the <see cref="ITestDatabaseSeedProfile"/> type to instantiate and use for seeding.
    /// </summary>
    public Type SeedProfileType { get; }
}
