using System.Collections.Generic;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Tour
{
    /// <summary>
    /// Represents a collection of <see cref="BackOfficeTourFilter"/> items.
    /// </summary>
    public class TourFilterCollection : BuilderCollectionBase<BackOfficeTourFilter>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TourFilterCollection"/> class.
        /// </summary>
        public TourFilterCollection(IEnumerable<BackOfficeTourFilter> items)
            : base(items)
        { }
    }
}
