namespace Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

/// <summary>
///     Allows for specifying custom DB types that are not natively mapped.
/// </summary>
public struct SpecialDbType : IEquatable<SpecialDbType>
{
    private readonly string _dbType;

    public SpecialDbType(string dbType)
    {
        if (string.IsNullOrWhiteSpace(dbType))
        {
            throw new ArgumentException($"'{nameof(dbType)}' cannot be null or whitespace.", nameof(dbType));
        }

        _dbType = dbType;
    }

    public SpecialDbType(SpecialDbTypes specialDbTypes)
        => _dbType = specialDbTypes.ToString();

    public static SpecialDbType NTEXT { get; } = new(SpecialDbTypes.NTEXT);

    public static SpecialDbType NCHAR { get; } = new(SpecialDbTypes.NCHAR);

    public static SpecialDbType NVARCHARMAX { get; } = new(SpecialDbTypes.NVARCHARMAX);

    // Make this directly castable to string
    public static implicit operator string(SpecialDbType dbType) => dbType.ToString();

    public override bool Equals(object? obj) => obj is SpecialDbType types && Equals(types);

    public bool Equals(SpecialDbType other) => _dbType == other._dbType;

    public override int GetHashCode() => 1038481724 + EqualityComparer<string>.Default.GetHashCode(_dbType);

    public override string ToString() => _dbType;

    // direct equality operators with SpecialDbTypes enum
    public static bool operator ==(SpecialDbTypes x, SpecialDbType y) => x.ToString() == y;

    public static bool operator !=(SpecialDbTypes x, SpecialDbType y) => x.ToString() != y;
}
