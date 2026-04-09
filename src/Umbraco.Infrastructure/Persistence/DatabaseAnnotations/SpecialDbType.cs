namespace Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

/// <summary>
///     Allows for specifying custom DB types that are not natively mapped.
/// </summary>
public struct SpecialDbType : IEquatable<SpecialDbType>
{
    private readonly string _dbType;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpecialDbType"/> class using the specified database type.
    /// </summary>
    /// <param name="dbType">A string representing the special database type.</param>
    public SpecialDbType(string dbType)
    {
        if (string.IsNullOrWhiteSpace(dbType))
        {
            throw new ArgumentException($"'{nameof(dbType)}' cannot be null or whitespace.", nameof(dbType));
        }

        _dbType = dbType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpecialDbType"/> class using the specified <see cref="SpecialDbTypes"/> value.
    /// </summary>
    /// <param name="specialDbTypes">A value indicating the special database types to be used by this instance.</param>
    public SpecialDbType(SpecialDbTypes specialDbTypes)
        => _dbType = specialDbTypes.ToString();

    /// <summary>
    /// Represents the NTEXT special database type, which was used for large Unicode text data in earlier versions of SQL Server.
    /// This member is obsolete; use <see cref="SpecialDbType.NVARCHARMAX"/> instead for new development, as NTEXT is deprecated in modern SQL Server versions.
    /// </summary>
    [Obsolete("Use NVARCHARMAX instead")]
    public static SpecialDbType NTEXT { get; } = new(SpecialDbTypes.NTEXT);

    /// <summary>
    /// Represents the special database type for a fixed-length Unicode character string (NCHAR).
    /// </summary>
    public static SpecialDbType NCHAR { get; } = new(SpecialDbTypes.NCHAR);

    /// <summary>
    /// Represents the special database type NVARCHAR(MAX), typically used for variable-length Unicode strings with no maximum length in SQL Server.
    /// </summary>
    public static SpecialDbType NVARCHARMAX { get; } = new(SpecialDbTypes.NVARCHARMAX);

    // Make this directly castable to string
    public static implicit operator string(SpecialDbType dbType) => dbType.ToString();

    /// <summary>
    /// Determines whether the specified object is equal to the current SpecialDbType instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns>true if the specified object is equal to the current instance; otherwise, false.</returns>
    public override bool Equals(object? obj) => obj is SpecialDbType types && Equals(types);

    /// <summary>Determines whether the specified <see cref="SpecialDbType"/> is equal to the current instance.</summary>
    /// <param name="other">The <see cref="SpecialDbType"/> to compare with the current instance.</param>
    /// <returns><c>true</c> if the specified <see cref="SpecialDbType"/> is equal to the current instance; otherwise, <c>false</c>.</returns>
    public bool Equals(SpecialDbType other) => _dbType == other._dbType;

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode() => 1038481724 + EqualityComparer<string>.Default.GetHashCode(_dbType);

    /// <summary>
    /// Returns the string representation of the special database type, as stored in the underlying <c>_dbType</c> field.
    /// </summary>
    /// <returns>The string value of the special database type.</returns>
    public override string ToString() => _dbType;

    // direct equality operators with SpecialDbTypes enum
    public static bool operator ==(SpecialDbTypes x, SpecialDbType y) => x.ToString() == y;

    public static bool operator !=(SpecialDbTypes x, SpecialDbType y) => x.ToString() != y;
}
