using Umbraco.Core.Hosting;

namespace Umbraco.Core.Configuration
{
    public interface IHostingSettings
    {
        bool DebugMode { get; }

        /// <summary>
        /// Gets the configuration for the location of temporary files.
        /// </summary>
        LocalTempStorage LocalTempStorageLocation { get; }

        /// <summary>
        /// Optional property to explicitly configure the application's virtual path
        /// </summary>
        /// <remarks>
        /// By default this is null which will mean that the <see cref="IHostingEnvironment.ApplicationVirtualPath"/> is automatically configured,
        /// otherwise this explicitly sets it. 
        /// If set, this value must begin with a "/" and cannot end with "/".
        /// </remarks>
        string ApplicationVirtualPath { get; }
    }
}
