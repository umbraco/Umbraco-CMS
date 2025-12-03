using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models.ContentEditing;

public abstract class ContentEditingModelBase
{
    public IEnumerable<PropertyValueModel> Properties { get; set; } = Array.Empty<PropertyValueModel>();

    public IEnumerable<VariantModel> Variants { get; set; } = Array.Empty<VariantModel>();

    /// <summary>
    /// Gets a dictionary of segments along with the cultures they are associated with.
    /// </summary>
    /// <param name="cultures">The cultures to consider when finding associated cultures for each segment.</param>
    /// <returns>
    /// A dictionary where the key is a unique segment from <see cref="Variants"/> and the value is
    /// the set of cultures that have at least one property defined for that segment in <see cref="Properties"/>.
    /// </returns>
    public Dictionary<string, HashSet<string>> GetPopulatedSegmentCultures(string[] cultures)
    {
        IEnumerable<string> uniqueSegments = Variants
            .Where(variant => variant.Segment is not null)
            .Select(variant => variant.Segment!)
            .Distinct();

        return uniqueSegments.ToDictionary(
            segment => segment,
            segment => Properties
                .Where(property => property.Segment.InvariantEquals(segment))
                .Where(property => property.Culture is not null && cultures.Contains(property.Culture))
                .Select(property => property.Culture!)
                .ToHashSet());
    }
}
