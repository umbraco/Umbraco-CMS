using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Web.Editors
{
    //TODO: find out where this should live
    public class TourFilterResolver
    {
        private static TourFilterResolver _current;

        private readonly HashSet<string> _disabledTours;

        public TourFilterResolver()
        {
            _disabledTours = new HashSet<string>();
        }

        public static TourFilterResolver Current
        {
            get { return _current ?? (_current = new TourFilterResolver()); }
        }

        public void Disable(string tour)
        {
            _disabledTours.Add(tour);
        }

        public string[] DisabledTours
        {
            get { return _disabledTours.ToArray(); }
        }
    }
}