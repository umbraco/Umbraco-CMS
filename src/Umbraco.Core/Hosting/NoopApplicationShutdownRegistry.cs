namespace Umbraco.Cms.Core.Hosting;

/// <summary>
/// A no-operation implementation of <see cref="IApplicationShutdownRegistry"/> that performs no actions.
/// </summary>
/// <remarks>
/// This implementation is used when shutdown registration is not required or supported,
/// such as in certain testing scenarios.
/// </remarks>
internal sealed class NoopApplicationShutdownRegistry : IApplicationShutdownRegistry
{
    /// <inheritdoc />
    public void RegisterObject(IRegisteredObject registeredObject)
    {
    }

    /// <inheritdoc />
    public void UnregisterObject(IRegisteredObject registeredObject)
    {
    }
}
