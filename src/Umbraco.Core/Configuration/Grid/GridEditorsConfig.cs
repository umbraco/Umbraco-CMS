using System;
using System.Collections.Generic;
using System.IO;
using Umbraco.Composing;
using Umbraco.Core.Cache;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Manifest;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Serialization;

namespace Umbraco.Core.Configuration.Grid
{
    internal class GridEditorsConfig : IGridEditorsConfig
    {
        private readonly AppCaches _appCaches;
        private readonly IIOHelper _ioHelper;
        private readonly IManifestParser _manifestParser;
        private readonly bool _isDebug;
        private readonly IJsonSerializer _jsonSerializer;

        public GridEditorsConfig(AppCaches appCaches, IIOHelper ioHelper, IManifestParser manifestParser,IJsonSerializer jsonSerializer, bool isDebug)
        {
            _appCaches = appCaches;
            _ioHelper = ioHelper;
            _manifestParser = manifestParser;
            _jsonSerializer = jsonSerializer;
            _isDebug = isDebug;
        }

        public IEnumerable<IGridEditorConfig> Editors
        {
            get
            {
                List<IGridEditorConfig> GetResult()
                {
                    var configFolder = new DirectoryInfo(_ioHelper.MapPath(Constants.SystemDirectories.Config));
                    var editors = new List<IGridEditorConfig>();
                    var gridConfig = Path.Combine(configFolder.FullName, "grid.editors.config.js");
                    if (File.Exists(gridConfig))
                    {
                        var sourceString = File.ReadAllText(gridConfig);

                        try
                        {
                            editors.AddRange(_jsonSerializer.Deserialize<IEnumerable<GridEditor>>(sourceString));
                        }
                        catch (Exception ex)
                        {
                            Current.Logger.Error<GridEditorsConfig>(ex, "Could not parse the contents of grid.editors.config.js into a JSON array '{Json}", sourceString);
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
                    : _appCaches.RuntimeCache.GetCacheItem<List<IGridEditorConfig>>(typeof(GridEditorsConfig) + ".Editors",GetResult, TimeSpan.FromMinutes(10));

                return result;
            }
        }
    }
}
