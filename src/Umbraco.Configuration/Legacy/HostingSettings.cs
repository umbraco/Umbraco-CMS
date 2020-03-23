using System.Configuration;

namespace Umbraco.Core.Configuration.Legacy
{
    public class HostingSettings : IHostingSettings
    {
        private bool? _debugMode;

        /// <inheritdoc />
        public LocalTempStorage LocalTempStorageLocation
        {
            get
            {
                var setting = ConfigurationManager.AppSettings[Constants.AppSettings.LocalTempStorage];
                if (!string.IsNullOrWhiteSpace(setting))
                    return Enum<LocalTempStorage>.Parse(setting);

                return LocalTempStorage.Default;
            }
        }

        /// <summary>
        /// Gets a value indicating whether umbraco is running in [debug mode].
        /// </summary>
        /// <value><c>true</c> if [debug mode]; otherwise, <c>false</c>.</value>
        public bool DebugMode
        {
            get
            {
                if (!_debugMode.HasValue)
                {
                    try
                    {
                        if (ConfigurationManager.GetSection("system.web/compilation") is ConfigurationSection compilation)
                        {
                            var debugElement = compilation.ElementInformation.Properties["debug"];

                            _debugMode = debugElement != null && (debugElement.Value is bool debug && debug);

                        }
                    }
                    catch
                    {
                        _debugMode = false;
                    }
                }

                return _debugMode.GetValueOrDefault();
            }
        }
    }
}
