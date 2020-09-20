using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;

namespace Umbraco.Tests.Common.Builders
{
    public class HostingSettingsBuilder : BuilderBase<HostingSettings>
    {
        private string _applicationVirtualPath;
        private bool? _debug;
        private string _localTempStorageLocation;

        public HostingSettingsBuilder WithApplicationVirtualPath(string applicationVirtualPath)
        {
            _applicationVirtualPath = applicationVirtualPath;
            return this;
        }

        public HostingSettingsBuilder WithDebug(bool debug)
        {
            _debug = debug;
            return this;
        }

        public HostingSettingsBuilder WithLocalTempStorageLocation(LocalTempStorage localTempStorageLocation)
        {
            _localTempStorageLocation = localTempStorageLocation.ToString();
            return this;
        }

        public HostingSettingsBuilder WithLocalTempStorageLocation(string localTempStorageLocation)
        {
            _localTempStorageLocation = localTempStorageLocation;
            return this;
        }

        public override HostingSettings Build()
        {
            var debug = _debug ?? false;
            var localTempStorageLocation = _localTempStorageLocation ?? LocalTempStorage.Default.ToString();
            var applicationVirtualPath = _applicationVirtualPath ?? null;

            return new HostingSettings
            {
                ApplicationVirtualPath = applicationVirtualPath,
                Debug = debug,
                LocalTempStorageLocation = localTempStorageLocation,                
            };
        }
    }
}
