using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Umbraco.Core;
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
        /// Helper method for adding a filter by exact plugin name
        /// </summary>
        /// <param name="pluginName">Regex string used for matching</param>
        public void AddFilterByPlugin(string pluginName)
        {
            pluginName = pluginName.EnsureStartsWith("^").EnsureEndsWith("$");
            using (Resolution.Configuration)
                _instances.Add(BackOfficeTourFilter.FilterPlugin(new Regex(pluginName, RegexOptions.IgnoreCase)));
        }

        /// <summary>
        /// Helper method for adding a filter by exact file name
        /// </summary>
        /// <param name="file"></param>
        public void AddFilterByFile(string file)
        {
            file = file.EnsureStartsWith("^").EnsureEndsWith("$");
            using (Resolution.Configuration)
                _instances.Add(BackOfficeTourFilter.FilterFile(new Regex(file, RegexOptions.IgnoreCase)));
        }

        /// <summary>
        /// Helper method for adding a filter by exact tour alias
        /// </summary>
        /// <param name="alias"></param>
        public void AddFilterByAlias(string alias)
        {
            alias = alias.EnsureStartsWith("^").EnsureEndsWith("$");
            using (Resolution.Configuration)
                _instances.Add(BackOfficeTourFilter.FilterAlias(new Regex(alias, RegexOptions.IgnoreCase)));
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