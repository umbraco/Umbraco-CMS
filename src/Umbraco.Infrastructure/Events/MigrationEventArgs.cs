using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.Cms.Core.Events;

public class MigrationEventArgs : CancellableObjectEventArgs<IList<Type>>, IEquatable<MigrationEventArgs>
{
    public MigrationEventArgs(IList<Type> migrationTypes, SemVersion configuredVersion, SemVersion targetVersion, string productName, bool canCancel)
        : this(migrationTypes, null, configuredVersion, targetVersion, productName, canCancel)
    {
    }

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
