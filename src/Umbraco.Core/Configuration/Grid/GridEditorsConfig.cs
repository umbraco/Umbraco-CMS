using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Manifest;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Configuration.Grid
{
    class GridEditorsConfig : IGridEditorsConfig
    {
        private readonly ILogger _logger;
        private readonly IRuntimeCacheProvider _runtimeCache;
        private readonly DirectoryInfo _appPlugins;
        private readonly DirectoryInfo _configFolder;
        private readonly bool _isDebug;

        public GridEditorsConfig(ILogger logger, IRuntimeCacheProvider runtimeCache, DirectoryInfo appPlugins, DirectoryInfo configFolder, bool isDebug)
        {
            _logger = logger;
            _runtimeCache = runtimeCache;
            _appPlugins = appPlugins;
            _configFolder = configFolder;
            _isDebug = isDebug;
        }

        public IEnumerable<IGridEditorConfig> Editors
        {
            get
            {
                Func<List<GridEditor>> getResult = () =>
                {
                    var editors = new List<GridEditor>();
                    var gridConfig = Path.Combine(_configFolder.FullName, "grid.editors.config.js");
                    if (File.Exists(gridConfig))
                    {
                        try
                        {
                            var arr = JArray.Parse(File.ReadAllText(gridConfig));
                            //ensure the contents parse correctly to objects
                            var parsed = ManifestParser.GetGridEditors(arr);
                            editors.AddRange(parsed);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error<GridEditorsConfig>("Could not parse the contents of grid.editors.config.js into a JSON array", ex);
                        }
                    }

                    var parser = new ManifestParser(_appPlugins, _runtimeCache);
                    var builder = new ManifestBuilder(_runtimeCache, parser);
                    foreach (var gridEditor in builder.GridEditors)
                    {
                        //no duplicates! (based on alias)
                        if (editors.Contains(gridEditor) == false)
                        {
                            editors.Add(gridEditor);
                        }
                    }
                    return editors;
                };

                //cache the result if debugging is disabled
                var result = _isDebug
                    ? getResult()
                    : _runtimeCache.GetCacheItem<List<GridEditor>>(
                        typeof(GridEditorsConfig) + "Editors",
                        () => getResult(),
                        TimeSpan.FromMinutes(10));

                return result;
            }
            
        }
    }
}