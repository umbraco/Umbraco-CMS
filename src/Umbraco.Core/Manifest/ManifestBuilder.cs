using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
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
        private readonly IRuntimeCacheProvider _runtimeCache;
        private readonly ManifestParser _parser;

        public ManifestBuilder(IRuntimeCacheProvider cache, ManifestParser parser)
        {
            _runtimeCache = cache;
            _parser = parser;
        }

        public const string GridEditorsKey = "gridEditors";
        public const string PropertyEditorsKey = "propertyEditors";
        public const string ParameterEditorsKey = "parameterEditors";
        public const string DashboardsKey = "dashboards";
        
        /// <summary>
        /// Returns all grid editors found in the manfifests
        /// </summary>
        internal IEnumerable<GridEditor> GridEditors
        {
            get
            {
                return _runtimeCache.GetCacheItem<IEnumerable<GridEditor>>(
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
                return _runtimeCache.GetCacheItem<IEnumerable<PropertyEditor>>(
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
                return _runtimeCache.GetCacheItem<IEnumerable<ParameterEditor>>(
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

        /// <summary>
        /// Returns all dashboards found in the manfifests
        /// </summary>
        internal IDictionary<string, Section> Dashboards
        {
            get
            {
                //TODO: Need to integrate the security with the manifest dashboards

                return _runtimeCache.GetCacheItem<IDictionary<string, Section>>(
                    typeof(ManifestBuilder) + DashboardsKey,
                    () =>
                    {
                        var dashboards = new Dictionary<string, Section>();
                        foreach (var manifest in _parser.GetManifests())
                        {
                            if (manifest.Dashboards != null)
                            {
                                var converted = manifest.Dashboards.ToDictionary(x => x.Key, x => x.Value.ToObject<Section>());
                                foreach (var item in converted)
                                {
                                    Section existing;
                                    if (dashboards.TryGetValue(item.Key, out existing))
                                    {
                                        foreach (var area in item.Value.Areas)
                                        {
                                            if (existing.Areas.Contains(area, StringComparer.InvariantCultureIgnoreCase) == false)
                                                existing.Areas.Add(area);
                                        }

                                        //merge
                                        foreach (var tab in item.Value.Tabs)
                                        {
                                            Tab existingTab;
                                            if (existing.Tabs.TryGetValue(tab.Key, out existingTab))
                                            {
                                                //merge
                                                foreach (var control in tab.Value.Controls)
                                                {
                                                    existingTab.Controls.Add(control);
                                                }
                                            }
                                            else
                                            {
                                                existing.Tabs[tab.Key] = tab.Value;
                                            }
                                        }
                                        ;
                                    }
                                    else
                                    {
                                        dashboards[item.Key] = item.Value;
                                    }
                                }
                            }
                        }
                        return dashboards;
                    }, new TimeSpan(0, 10, 0));
            }
        }

        #region Internal manifest models
        internal class Section
        {
            public Section()
            {
                Areas = new List<string>();
                Tabs = new Dictionary<string, Tab>();
            }
            [JsonProperty("areas")]
            public List<string> Areas { get; set; }
            [JsonProperty("tabs")]
            public IDictionary<string, Tab> Tabs { get; set; }
        }

        internal class Tab
        {
            public Tab()
            {
                Controls = new List<Control>();
                Index = int.MaxValue; //default so we can check if this value has been explicitly set
            }
            [JsonProperty("controls")]
            public List<Control> Controls { get; set; }
            [JsonProperty("index")]
            public int Index { get; set; }
        }

        internal class Control
        {
            [JsonProperty("path")]
            public string Path { get; set; }
            [JsonProperty("caption")]
            public string Caption { get; set; }
        } 
        #endregion

    }
}
