namespace Umbraco.Core.Logging
{

    public interface ILoggingConfiguration
    {
        /// <summary>
        /// The physical path where logs are stored
        /// </summary>
        string LogDirectory { get; }
        string LogConfigurationFile { get; }
        string UserLogConfigurationFile { get; }
    }
}
