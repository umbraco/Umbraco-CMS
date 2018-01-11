using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;
using Umbraco.Web.Models;

namespace Umbraco.Web
{
    /// <summary>
    /// Allows for adding filters for tours during startup
    /// </summary>
    public class TourFilterResolver : ManyObjectsResolverBase<TourFilterResolver, BackOfficeTourFilter>
    {
        public TourFilterResolver(IServiceProvider serviceProvider, ILogger logger) : base(serviceProvider, logger)
        {
        }

        private readonly HashSet<BackOfficeTourFilter> _instances = new HashSet<BackOfficeTourFilter>();

        public IEnumerable<BackOfficeTourFilter> Filters
        {
            get { return Values; }
        }

        /// <summary>
        /// Adds a filter instance
        /// </summary>
        /// <param name="filter"></param>
        public void AddFilter(BackOfficeTourFilter filter)
        {
            using (Resolution.Configuration)
                _instances.Add(filter);
        }

        /// <summary>
        /// Removes a filter instance
        /// </summary>
        /// <param name="filter"></param>
        public void RemoveFilter(BackOfficeTourFilter filter)
        {
            using (Resolution.Configuration)
                _instances.Remove(filter);
        }

        /// <summary>
        /// Removes a filter instance based on callback
        /// </summary>
        /// <param name="filter"></param>
        public void RemoveFilterWhere(Func<BackOfficeTourFilter, bool> filter)
        {
            using (Resolution.Configuration)
                _instances.RemoveWhere(new Predicate<BackOfficeTourFilter>(filter));
        }

        /// <inheritdoc />
        /// <summary>
        /// Overridden to return the combined created instances based on the resolved Types and the Concrete values added with AddFilter
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<BackOfficeTourFilter> CreateInstances()
        {
            var createdInstances = base.CreateInstances();
            return createdInstances.Concat(_instances);
        }

        public override void Clear()
        {
            base.Clear();
            _instances.Clear();
        }

        internal override void ResetCollections()
        {
            base.ResetCollections();
            _instances.Clear();
        }
    }
}