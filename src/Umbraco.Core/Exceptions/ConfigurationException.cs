namespace Umbraco.Cms.Core.Exceptions;

/// <summary>
///     An exception that is thrown if the configuration is wrong.
/// </summary>
/// <seealso cref="System.Exception" />
public class ConfigurationException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigurationException" /> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ConfigurationException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigurationException" /> class with a specified error message
    ///     and a reference to the inner exception which is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The inner exception, or null.</param>
    public ConfigurationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
