namespace Umbraco.Cms.Core;

/// <summary>
///     Represents a UDI with an unknown entity type.
/// </summary>
/// <remarks>
///     This class is used when parsing UDIs with unknown entity types when using the <c>knownTypes</c> parameter.
/// </remarks>
public class UnknownTypeUdi : Udi
{
    /// <summary>
    ///     Gets the singleton instance of the <see cref="UnknownTypeUdi" /> class.
    /// </summary>
    public static readonly UnknownTypeUdi Instance = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="UnknownTypeUdi" /> class.
    /// </summary>
    private UnknownTypeUdi()
        : base("unknown", "umb://unknown/")
    {
    }

    /// <inheritdoc />
    public override bool IsRoot => false;
}
