using Microsoft.Extensions.Configuration;
using Umbraco.Core.Configuration;

namespace Umbraco.Configuration.Models
{
    public class HostingSettings : IHostingSettings
    {
        private readonly IConfiguration _configuration;

        public HostingSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <inheritdoc />
        public LocalTempStorage LocalTempStorageLocation =>
            _configuration.GetValue("Umbraco:CMS:Hosting:LocalTempStorage", LocalTempStorage.Default);

        /// <summary>
        ///     Gets a value indicating whether umbraco is running in [debug mode].
        /// </summary>
        /// <value><c>true</c> if [debug mode]; otherwise, <c>false</c>.</value>
        public bool DebugMode => _configuration.GetValue("Umbraco:CMS:Hosting:Debug", false);
    }
}
