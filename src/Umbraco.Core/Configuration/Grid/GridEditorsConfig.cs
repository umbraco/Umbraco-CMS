using System;
using System.Collections.Generic;
using System.IO;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Manifest;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Configuration.Grid
{
    internal class GridEditorsConfig : IGridEditorsConfig
    {
        private readonly ILogger _logger;
        private readonly AppCaches _appCaches;
        private readonly DirectoryInfo _configFolder;
        private readonly ManifestParser _manifestParser;
        private readonly bool _isDebug;

        public GridEditorsConfig(ILogger logger, AppCaches appCaches, DirectoryInfo configFolder, ManifestParser manifestParser, bool isDebug)
        {
            _logger = logger;
            _appCaches = appCaches;
            _configFolder = configFolder;
            _manifestParser = manifestParser;
            _isDebug = isDebug;
        }

        public IEnumerable<IGridEditorConfig> Editors
        {
            get
            {
                List<GridEditor> GetResult()
                {
                    var editors = new List<GridEditor>();
                    var gridConfig = Path.Combine(_configFolder.FullName, "grid.editors.config.js");
                    if (File.Exists(gridConfig))
                    {
                        var sourceString = File.ReadAllText(gridConfig);

                        try
                        {
                            editors.AddRange(_manifestParser.ParseGridEditors(sourceString));
                        }
                        catch (Exception ex)
                        {
                            _logger.Error<GridEditorsConfig,string>(ex, "Could not parse the contents of grid.editors.config.js into a JSON array '{Json}", sourceString);
                        }
                    }

                    // add manifest editors, skip duplicates
                    foreach (var gridEditor in _manifestParser.Manifest.GridEditors)
                    {
                        if (editors.Contains(gridEditor) == false) editors.Add(gridEditor);
                    }

                    return editors;
                }

                //cache the result if debugging is disabled
                var result = _isDebug
                    ? GetResult()
                    : _appCaches.RuntimeCache.GetCacheItem<List<GridEditor>>(typeof(GridEditorsConfig) + ".Editors",GetResult, TimeSpan.FromMinutes(10));

                return result;
            }

        }
    }
}
