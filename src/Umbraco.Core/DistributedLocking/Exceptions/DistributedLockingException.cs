namespace Umbraco.Cms.Core.DistributedLocking.Exceptions;

/// <summary>
///     Base class for all DistributedLockingExceptions.
/// </summary>
public class DistributedLockingException : ApplicationException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DistributedLockingException" /> class.
    /// </summary>
    public DistributedLockingException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DistributedLockingException" /> class.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public DistributedLockingException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
