using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Umbraco.Core.Cache;
using Umbraco.Core.IO;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Manifest
{
    /// <summary>
    /// This reads in the manifests and stores some definitions in memory so we can look them on the server side
    /// </summary>
    internal class ManifestBuilder
    {
        private readonly IRuntimeCacheProvider _cache;
        private readonly ManifestParser _parser;

        public ManifestBuilder(IRuntimeCacheProvider cache, ManifestParser parser)
        {
            _cache = cache;
            _parser = parser;
        }

        private const string GridEditorsKey = "grideditors";
        private const string PropertyEditorsKey = "propertyeditors";
        private const string ParameterEditorsKey = "parametereditors";

        /// <summary>
        /// Returns all grid editors found in the manfifests
        /// </summary>
        internal IEnumerable<GridEditor> GridEditors
        {
            get
            {
                return _cache.GetCacheItem<IEnumerable<GridEditor>>(
                    typeof (ManifestBuilder) + GridEditorsKey,
                    () =>
                    {
                        var editors = new List<GridEditor>();
                        foreach (var manifest in _parser.GetManifests())
                        {
                            if (manifest.GridEditors != null)
                            {
                                editors.AddRange(ManifestParser.GetGridEditors(manifest.GridEditors));
                            }

                        }
                        return editors;
                    }, new TimeSpan(0, 10, 0));
            }
        }

        /// <summary>
        /// Returns all property editors found in the manfifests
        /// </summary>
        internal IEnumerable<PropertyEditor> PropertyEditors
        {
            get
            {
                return _cache.GetCacheItem<IEnumerable<PropertyEditor>>(
                    typeof(ManifestBuilder) + PropertyEditorsKey,
                    () =>
                    {
                        var editors = new List<PropertyEditor>();
                        foreach (var manifest in _parser.GetManifests())
                        {
                            if (manifest.PropertyEditors != null)
                            {
                                editors.AddRange(ManifestParser.GetPropertyEditors(manifest.PropertyEditors));
                            }

                        }
                        return editors;
                    }, new TimeSpan(0, 10, 0));
            }
        }

        /// <summary>
        /// Returns all parameter editors found in the manfifests and all property editors that are flagged to be parameter editors
        /// </summary>
        internal IEnumerable<ParameterEditor> ParameterEditors
        {
            get
            {
                return _cache.GetCacheItem<IEnumerable<ParameterEditor>>(
                    typeof (ManifestBuilder) + ParameterEditorsKey,
                    () =>
                    {
                        var editors = new List<ParameterEditor>();
                        foreach (var manifest in _parser.GetManifests())
                        {
                            if (manifest.ParameterEditors != null)
                            {
                                editors.AddRange(ManifestParser.GetParameterEditors(manifest.ParameterEditors));
                            }
                        }
                        return editors;
                    }, new TimeSpan(0, 10, 0));
            }
        } 
        
    }
}