using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.Cms.Core.Events;

/// <summary>
/// Provides data for events that occur during Umbraco database migrations.
/// </summary>
public class MigrationEventArgs : CancellableObjectEventArgs<IList<Type>>, IEquatable<MigrationEventArgs>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Core.Events.MigrationEventArgs"/> class.
    /// </summary>
    /// <param name="migrationTypes">A list of migration types that are part of the migration process.</param>
    /// <param name="configuredVersion">The version currently configured before migration begins.</param>
    /// <param name="targetVersion">The version to which the migration will be performed.</param>
    /// <param name="productName">The name of the product undergoing migration.</param>
    /// <param name="canCancel">True if the migration process can be cancelled; otherwise, false.</param>
    public MigrationEventArgs(IList<Type> migrationTypes, SemVersion configuredVersion, SemVersion targetVersion, string productName, bool canCancel)
        : this(migrationTypes, null, configuredVersion, targetVersion, productName, canCancel)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Core.Events.MigrationEventArgs"/> class.
    /// </summary>
    /// <param name="migrationTypes">A list of migration types that are being executed.</param>
    /// <param name="configuredVersion">The version configured before the migration starts.</param>
    /// <param name="targetVersion">The version to which the migration is being applied.</param>
    /// <param name="productName">The name of the product undergoing migration.</param>
    public MigrationEventArgs(IList<Type> migrationTypes, SemVersion configuredVersion, SemVersion targetVersion, string productName)
        : this(migrationTypes, null, configuredVersion, targetVersion, productName, false)
    {
    }

    internal MigrationEventArgs(IList<Type> migrationTypes, IMigrationContext? migrationContext, SemVersion configuredVersion, SemVersion targetVersion, string productName, bool canCancel)
        : base(migrationTypes, canCancel)
    {
        MigrationContext = migrationContext;
        ConfiguredSemVersion = configuredVersion;
        TargetSemVersion = targetVersion;
        ProductName = productName;
    }

    /// <summary>
    ///     Returns all migrations that were used in the migration runner
    /// </summary>
    public IList<Type>? MigrationsTypes => EventObject;

    /// <summary>
    ///     Gets the origin version of the migration, i.e. the one that is currently installed.
    /// </summary>
    public SemVersion ConfiguredSemVersion { get; }

    /// <summary>
    ///     Gets the target version of the migration.
    /// </summary>
    public SemVersion TargetSemVersion { get; }

    /// <summary>
    ///     Gets the product name.
    /// </summary>
    public string ProductName { get; }

    /// <summary>
    ///     Gets the migration context.
    /// </summary>
    /// <remarks>Is only available after migrations have run, for post-migrations.</remarks>
    internal IMigrationContext? MigrationContext { get; }

    public static bool operator ==(MigrationEventArgs left, MigrationEventArgs right) => Equals(left, right);

    public static bool operator !=(MigrationEventArgs left, MigrationEventArgs right) => !Equals(left, right);

    /// <summary>
    /// Determines whether the current <see cref="Umbraco.Cms.Core.Events.MigrationEventArgs"/> instance is equal to another instance of the same type.
    /// </summary>
    /// <param name="other">The <see cref="Umbraco.Cms.Core.Events.MigrationEventArgs"/> to compare with the current instance.</param>
    /// <returns><c>true</c> if the specified instance is equal to the current instance; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Equality is determined by comparing the values of <c>ConfiguredSemVersion</c>, <c>MigrationContext</c>, <c>ProductName</c>, and <c>TargetSemVersion</c>.
    /// </remarks>
    public bool Equals(MigrationEventArgs? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return base.Equals(other) && ConfiguredSemVersion.Equals(other.ConfiguredSemVersion) &&
               (MigrationContext?.Equals(other.MigrationContext) ?? false) &&
               string.Equals(ProductName, other.ProductName) && TargetSemVersion.Equals(other.TargetSemVersion);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current <see cref="MigrationEventArgs"/> instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><c>true</c> if the specified object is equal to the current instance; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((MigrationEventArgs)obj);
    }

    /// <summary>
    /// Returns a hash code for this instance, based on its property values.
    /// </summary>
    /// <returns>A hash code for the current <see cref="MigrationEventArgs"/> object.</returns>
    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = base.GetHashCode();
            hashCode = (hashCode * 397) ^ ConfiguredSemVersion.GetHashCode();
            if (MigrationContext is not null)
            {
                hashCode = (hashCode * 397) ^ MigrationContext.GetHashCode();
            }

            hashCode = (hashCode * 397) ^ ProductName.GetHashCode();
            hashCode = (hashCode * 397) ^ TargetSemVersion.GetHashCode();
            return hashCode;
        }
    }
}
