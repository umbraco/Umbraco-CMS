using System.IO;
using Umbraco.Core.Cache;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Manifest;
using Umbraco.Core.Serialization;

namespace Umbraco.Core.Configuration.Grid
{
    public class GridConfig : IGridConfig
    {
        public GridConfig(AppCaches appCaches, IIOHelper ioHelper, IManifestParser manifestParser, IJsonSerializer jsonSerializer, IHostingEnvironment hostingEnvironment)
        {
            EditorsConfig = new GridEditorsConfig(appCaches, ioHelper, manifestParser, jsonSerializer, hostingEnvironment.IsDebugMode);
        }

        public IGridEditorsConfig EditorsConfig { get; }
    }
}
