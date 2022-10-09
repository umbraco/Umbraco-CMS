using Umbraco.Cms.Core.Hosting;

namespace Umbraco.Cms.Core.Runtime;

/// <summary>
///     Represents the main AppDomain running for a given application.
/// </summary>
/// <remarks>
///     <para>There can be only one "main" AppDomain running for a given application at a time.</para>
///     <para>It is possible to register against the MainDom and be notified when it is released.</para>
/// </remarks>
public interface IMainDom
{
    /// <summary>
    ///     Gets a value indicating whether the current domain is the main domain.
    /// </summary>
    /// <remarks>
    ///     Acquire must be called first else this will always return false
    /// </remarks>
    bool IsMainDom { get; }

    /// <summary>
    ///     Tries to acquire the MainDom, returns true if successful else false
    /// </summary>
    bool Acquire(IApplicationShutdownRegistry hostingEnvironment);

    /// <summary>
    ///     Registers a resource that requires the current AppDomain to be the main domain to function.
    /// </summary>
    /// <param name="install">An action to execute when registering.</param>
    /// <param name="release">An action to execute before the AppDomain releases the main domain status.</param>
    /// <param name="weight">An optional weight (lower goes first).</param>
    /// <returns>A value indicating whether it was possible to register.</returns>
    /// <remarks>
    ///     If registering is successful, then the <paramref name="install" /> action
    ///     is guaranteed to execute before the AppDomain releases the main domain status.
    /// </remarks>
    bool Register(Action? install = null, Action? release = null, int weight = 100);
}
