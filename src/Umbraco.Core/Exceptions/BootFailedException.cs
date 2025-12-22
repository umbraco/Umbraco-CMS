using System.Text;

namespace Umbraco.Cms.Core.Exceptions;

/// <summary>
///     An exception that is thrown if the Umbraco application cannot boot.
/// </summary>
/// <seealso cref="System.Exception" />
public class BootFailedException : Exception
{
    /// <summary>
    ///     Defines the default boot failed exception message.
    /// </summary>
    public const string DefaultMessage = "Boot failed: Umbraco cannot run. See Umbraco's log file for more details.";

    /// <summary>
    ///     Initializes a new instance of the <see cref="BootFailedException" /> class.
    /// </summary>
    public BootFailedException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="Exception" /> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public BootFailedException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="Exception" /> class with a specified error message
    ///     and a reference to the inner exception which is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The inner exception, or null.</param>
    public BootFailedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    ///     Rethrows a captured <see cref="BootFailedException" />.
    /// </summary>
    /// <param name="bootFailedException">The boot failed exception.</param>
    /// <exception cref="BootFailedException">
    /// </exception>
    /// <remarks>
    ///     The exception can be null, in which case a default message is used.
    /// </remarks>
    public static void Rethrow(BootFailedException? bootFailedException)
    {
        if (bootFailedException == null)
        {
            throw new BootFailedException(DefaultMessage);
        }

        // see https://stackoverflow.com/questions/57383
        // would that be the correct way to do it?
        // ExceptionDispatchInfo.Capture(bootFailedException).Throw();
        Exception? e = bootFailedException;
        var m = new StringBuilder();
        m.Append(DefaultMessage);
        while (e != null)
        {
            m.Append($"\n\n-> {e.GetType().FullName}: {e.Message}");
            if (string.IsNullOrWhiteSpace(e.StackTrace) == false)
            {
                m.Append($"\n{e.StackTrace}");
            }

            e = e.InnerException;
        }

        throw new BootFailedException(m.ToString());
    }
}
