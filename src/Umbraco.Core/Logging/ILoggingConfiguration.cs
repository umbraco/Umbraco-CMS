namespace Umbraco.Cms.Core.Logging;

public interface ILoggingConfiguration
{
    /// <summary>
    ///     Gets the physical path where logs are stored
    /// </summary>
    string LogDirectory { get; }
}
