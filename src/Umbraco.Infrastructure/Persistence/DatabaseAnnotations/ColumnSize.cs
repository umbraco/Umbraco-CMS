namespace Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

/// <summary>
/// Type that is necessary for representation of size of sql columns
/// </summary>
public struct ColumnSize : IEquatable<ColumnSize>
{
    private const string MaxValue = "max";

    /// <summary>
    /// Gets or sets the size/length of a column
    /// </summary>
    public int Size { get; }

    /// <summary>
    /// Gets or sets type of column site
    /// </summary>
    /// <remarks>Type can be either numeric or constant (f.e. max type)</remarks>
    public ColumnSizeType Type { get; }

    /// <summary>
    /// Returns default ColumnSize
    /// </summary>
    /// <remarks>Default ColumnSize has 0 as Size and numeric type</remarks>
    public static ColumnSize Default { get => new(default); }

    public ColumnSize(int size)
    {
        Size = size;
        Type = ColumnSizeType.Numeric;
    }

    public ColumnSize(ColumnSizeType type, int size = default)
    {
        Type = type;
        Size = size;
    }

    public bool Equals(ColumnSize other) => Size == other.Size && Type == other.Type;

    public override bool Equals(object? obj)
    {
        return obj is ColumnSize other && Equals(other);
    }

    public static implicit operator ColumnSize(int b) => new(b);

    public static bool operator ==(ColumnSize columnSize1, ColumnSize columnSize2) => columnSize1.Equals(columnSize2);

    public static bool operator !=(ColumnSize columnSize1, ColumnSize columnSize2) => !columnSize1.Equals(columnSize2);

    public override string ToString() => Type == ColumnSizeType.Max ? MaxValue : Size.ToString();
}

public enum ColumnSizeType
{
    Numeric,
    Max
}
