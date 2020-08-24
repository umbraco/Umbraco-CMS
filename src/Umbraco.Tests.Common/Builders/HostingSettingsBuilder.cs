using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;

namespace Umbraco.Tests.Common.Builders
{
    public class HostingSettingsBuilder : BuilderBase<HostingSettings>
    {
        private string _applicationVirtualPath;
        private bool? _debugMode;
        private LocalTempStorage? _localTempStorageLocation;

        public HostingSettingsBuilder WithApplicationVirtualPath(string applicationVirtualPath)
        {
            _applicationVirtualPath = applicationVirtualPath;
            return this;
        }

        public HostingSettingsBuilder WithDebugMode(bool debugMode)
        {
            _debugMode = debugMode;
            return this;
        }

        public HostingSettingsBuilder WithLocalTempStorageLocation(LocalTempStorage localTempStorageLocation)
        {
            _localTempStorageLocation = localTempStorageLocation;
            return this;
        }

        public override HostingSettings Build()
        {
            var debugMode = _debugMode ?? false;
            var localTempStorageLocation = _localTempStorageLocation ?? LocalTempStorage.Default;
            var applicationVirtualPath = _applicationVirtualPath ?? null;

            return new HostingSettings
            {
                ApplicationVirtualPath = applicationVirtualPath,
                DebugMode = debugMode,
                LocalTempStorageLocation = localTempStorageLocation,                
            };
        }
    }
}
