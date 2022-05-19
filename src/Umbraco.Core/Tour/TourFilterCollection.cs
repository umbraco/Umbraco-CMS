using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Tour;

/// <summary>
///     Represents a collection of <see cref="BackOfficeTourFilter" /> items.
/// </summary>
public class TourFilterCollection : BuilderCollectionBase<BackOfficeTourFilter>
{
    public TourFilterCollection(Func<IEnumerable<BackOfficeTourFilter>> items)
        : base(items)
    {
    }
}
