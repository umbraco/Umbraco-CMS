namespace Umbraco.Cms.Core.Models.Editors;

/// <summary>
///     Used to track reference to other entities in a property value
/// </summary>
public struct UmbracoEntityReference : IEquatable<UmbracoEntityReference>
{
    private static readonly UmbracoEntityReference _empty = new(UnknownTypeUdi.Instance, string.Empty);

    public UmbracoEntityReference(Udi udi, string relationTypeAlias)
    {
        Udi = udi ?? throw new ArgumentNullException(nameof(udi));
        RelationTypeAlias = relationTypeAlias ?? throw new ArgumentNullException(nameof(relationTypeAlias));
    }

    public UmbracoEntityReference(Udi udi)
    {
        Udi = udi ?? throw new ArgumentNullException(nameof(udi));

        switch (udi.EntityType)
        {
            case Constants.UdiEntityType.Media:
                RelationTypeAlias = Constants.Conventions.RelationTypes.RelatedMediaAlias;
                break;
            default:
                RelationTypeAlias = Constants.Conventions.RelationTypes.RelatedDocumentAlias;
                break;
        }
    }

    public Udi Udi { get; }

    public static UmbracoEntityReference Empty() => _empty;

    public static bool IsEmpty(UmbracoEntityReference reference) => reference == Empty();

    public string RelationTypeAlias { get; }

    public static bool operator ==(UmbracoEntityReference left, UmbracoEntityReference right) => left.Equals(right);

    public override bool Equals(object? obj) => obj is UmbracoEntityReference reference && Equals(reference);

    public bool Equals(UmbracoEntityReference other) =>
        EqualityComparer<Udi>.Default.Equals(Udi, other.Udi) &&
        RelationTypeAlias == other.RelationTypeAlias;

    public override int GetHashCode()
    {
        var hashCode = -487348478;
        hashCode = (hashCode * -1521134295) + EqualityComparer<Udi>.Default.GetHashCode(Udi);
        hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(RelationTypeAlias);
        return hashCode;
    }

    public static bool operator !=(UmbracoEntityReference left, UmbracoEntityReference right) => !(left == right);
}
