namespace Umbraco.Cms.Core;

/// <summary>
///     Represents a <see cref="Core.Udi" /> range.
/// </summary>
/// <remarks>
///     <remarks>
///         A Udi range is composed of a <see cref="Core.Udi" /> which represents the base of the range,
///         plus a selector that can be "." (the Udi), ".*" (the Udi and its children), ".**" (the udi and
///         its descendants, "*" (the children of the Udi), and "**" (the descendants of the Udi).
///     </remarks>
///     <remarks>The Udi here can be a closed entity, or an open entity.</remarks>
/// </remarks>
public class UdiRange
{
    private readonly Uri _uriValue;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UdiRange" /> class with a <see cref="Core.Udi" /> and an optional
    ///     selector.
    /// </summary>
    /// <param name="udi">A <see cref="Udi" />.</param>
    /// <param name="selector">An optional selector.</param>
    public UdiRange(Udi udi, string selector = Constants.DeploySelector.This)
    {
        Udi = udi;
        switch (selector)
        {
            case Constants.DeploySelector.This:
                Selector = selector;
                _uriValue = udi.UriValue;
                break;
            case Constants.DeploySelector.ChildrenOfThis:
            case Constants.DeploySelector.DescendantsOfThis:
            case Constants.DeploySelector.ThisAndChildren:
            case Constants.DeploySelector.ThisAndDescendants:
            case Constants.DeploySelector.EntitiesOfType:
                Selector = selector;
                _uriValue = new Uri(Udi + "?" + selector);
                break;
            default:
                throw new ArgumentException(string.Format("Invalid selector \"{0}\".", selector));
        }
    }

    /// <summary>
    ///     Gets the <see cref="Udi" /> for this range.
    /// </summary>
    public Udi Udi { get; }

    /// <summary>
    ///     Gets or sets the selector for this range.
    /// </summary>
    public string Selector { get; }

    /// <summary>
    ///     Gets the entity type of the <see cref="Core.Udi" /> for this range.
    /// </summary>
    public string EntityType => Udi.EntityType;

    public static bool operator ==(UdiRange? range1, UdiRange? range2)
    {
        if (ReferenceEquals(range1, range2))
        {
            return true;
        }

        if (range1 is null || range2 is null)
        {
            return false;
        }

        return range1.Equals(range2);
    }

    public static bool operator !=(UdiRange range1, UdiRange range2) => !(range1 == range2);

    public static UdiRange Parse(string value)
    {
        if (Uri.TryCreate(value, UriKind.Absolute, out Uri? uri) is false ||
            uri.IsWellFormedOriginalString() is false)
        {
            throw new FormatException($"String \"{value}\" is not a valid UDI range.");
        }

        // Remove selector from UDI
        Uri udiUri = string.IsNullOrEmpty(uri.Query)
            ? uri
            : new UriBuilder(uri) { Query = string.Empty }.Uri;

        var udi = Udi.Create(udiUri);

        // Only specify selector if query string is not empty
        return string.IsNullOrEmpty(uri.Query)
            ? new UdiRange(udi)
            : new UdiRange(udi, uri.Query.TrimStart(Constants.CharArrays.QuestionMark));
    }

    public override string ToString() => _uriValue.ToString();

    public override bool Equals(object? obj) =>
        obj is UdiRange other && GetType() == other.GetType() && _uriValue == other._uriValue;

    public override int GetHashCode() => _uriValue.GetHashCode();
}
