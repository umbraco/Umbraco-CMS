﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Tour
{
    /// <summary>
    /// Builds a collection of <see cref="BackOfficeTourFilter"/> items.
    /// </summary>
    public class TourFilterCollectionBuilder : CollectionBuilderBase<TourFilterCollectionBuilder, TourFilterCollection, BackOfficeTourFilter>
    {
        private readonly HashSet<BackOfficeTourFilter> _instances = new HashSet<BackOfficeTourFilter>();

        /// <inheritdoc />
        protected override IEnumerable<BackOfficeTourFilter> CreateItems(IFactory factory)
        {
            return base.CreateItems(factory).Concat(_instances);
        }

        /// <summary>
        /// Adds a filter instance.
        /// </summary>
        public void AddFilter(BackOfficeTourFilter filter)
        {
            _instances.Add(filter);
        }

        /// <summary>
        /// Removes a filter instance.
        /// </summary>
        public void RemoveFilter(BackOfficeTourFilter filter)
        {
            _instances.Remove(filter);
        }

        /// <summary>
        /// Removes all filter instances.
        /// </summary>
        public void RemoveAllFilters()
        {
            _instances.Clear();
        }

        /// <summary>
        /// Removes filters matching a condition.
        /// </summary>
        public void RemoveFilter(Func<BackOfficeTourFilter, bool> predicate)
        {
            _instances.RemoveWhere(new Predicate<BackOfficeTourFilter>(predicate));
        }

        /// <summary>
        /// Creates and adds a filter instance filtering by plugin name.
        /// </summary>
        public void AddFilterByPlugin(string pluginName)
        {
            pluginName = pluginName.EnsureStartsWith("^").EnsureEndsWith("$");
            _instances.Add(BackOfficeTourFilter.FilterPlugin(new Regex(pluginName, RegexOptions.IgnoreCase)));
        }

        /// <summary>
        /// Creates and adds a filter instance filtering by tour filename.
        /// </summary>
        public void AddFilterByFile(string filename)
        {
            filename = filename.EnsureStartsWith("^").EnsureEndsWith("$");
            _instances.Add(BackOfficeTourFilter.FilterFile(new Regex(filename, RegexOptions.IgnoreCase)));
        }
    }
}
