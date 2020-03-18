namespace Umbraco.Core.Configuration
{
    public interface IHostingSettings
    {
        bool DebugMode { get; }
        /// <summary>
        /// Gets the configuration for the location of temporary files.
        /// </summary>
        LocalTempStorage LocalTempStorageLocation { get; }
    }
}
