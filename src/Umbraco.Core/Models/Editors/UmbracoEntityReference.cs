namespace Umbraco.Cms.Core.Models.Editors;

/// <summary>
/// Used to track a reference to another entity in a property value.
/// </summary>
public struct UmbracoEntityReference : IEquatable<UmbracoEntityReference>
{
    private static readonly UmbracoEntityReference _empty = new(UnknownTypeUdi.Instance, string.Empty);

    /// <summary>
    /// Initializes a new instance of the <see cref="UmbracoEntityReference" /> struct.
    /// </summary>
    /// <param name="udi">The UDI.</param>
    /// <param name="relationTypeAlias">The relation type alias.</param>
    public UmbracoEntityReference(Udi udi, string relationTypeAlias)
    {
        Udi = udi ?? throw new ArgumentNullException(nameof(udi));
        RelationTypeAlias = relationTypeAlias ?? throw new ArgumentNullException(nameof(relationTypeAlias));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UmbracoEntityReference" /> struct for a document or media item.
    /// </summary>
    /// <param name="udi">The UDI.</param>
    public UmbracoEntityReference(Udi udi)
    {
        Udi = udi ?? throw new ArgumentNullException(nameof(udi));

        switch (udi.EntityType)
        {
            case Constants.UdiEntityType.Document:
                RelationTypeAlias = Constants.Conventions.RelationTypes.RelatedDocumentAlias;
                break;
            case Constants.UdiEntityType.Media:
                RelationTypeAlias = Constants.Conventions.RelationTypes.RelatedMediaAlias;
                break;
            default:
                // No relation type alias convention for this entity type, so leave it empty
                RelationTypeAlias = string.Empty;
                break;
        }
    }

    /// <summary>
    /// Gets the UDI.
    /// </summary>
    /// <value>
    /// The UDI.
    /// </value>
    public Udi Udi { get; }

    /// <summary>
    /// Gets the relation type alias.
    /// </summary>
    /// <value>
    /// The relation type alias.
    /// </value>
    public string RelationTypeAlias { get; }

    /// <summary>
    /// Gets an empty reference.
    /// </summary>
    /// <returns>
    /// An empty reference.
    /// </returns>
    public static UmbracoEntityReference Empty() => _empty;

    /// <summary>
    /// Determines whether the specified reference is empty.
    /// </summary>
    /// <param name="reference">The reference.</param>
    /// <returns>
    ///   <c>true</c> if the specified reference is empty; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsEmpty(UmbracoEntityReference reference) => reference == Empty();

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is UmbracoEntityReference reference && Equals(reference);

    /// <inheritdoc />
    public bool Equals(UmbracoEntityReference other) =>
        EqualityComparer<Udi>.Default.Equals(Udi, other.Udi) &&
        RelationTypeAlias == other.RelationTypeAlias;

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hashCode = -487348478;
        hashCode = (hashCode * -1521134295) + EqualityComparer<Udi>.Default.GetHashCode(Udi);
        hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(RelationTypeAlias);
        return hashCode;
    }

    /// <inheritdoc />
    public static bool operator ==(UmbracoEntityReference left, UmbracoEntityReference right) => left.Equals(right);

    /// <inheritdoc />
    public static bool operator !=(UmbracoEntityReference left, UmbracoEntityReference right) => !(left == right);
}
