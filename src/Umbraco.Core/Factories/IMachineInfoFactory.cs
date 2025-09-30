namespace Umbraco.Cms.Core.Factories;

/// <summary>
/// Fetches information of the host machine.
/// </summary>
public interface IMachineInfoFactory
{
    /// <summary>
    /// Fetches the name of the Host Machine for identification.
    /// </summary>
    /// <returns>A name of the host machine.</returns>
    public string GetMachineIdentifier();

    /// <summary>
    /// Gets the local identity for the executing AppDomain.
    /// </summary>
    /// <remarks>
    /// <para>
    ///         It is not only about the "server" (machine name and appDomainappId), but also about
    ///         an AppDomain, within a Process, on that server - because two AppDomains running at the same
    ///         time on the same server (eg during a restart) are, practically, a LB setup.
    ///     </para>
    ///     <para>
    ///         Practically, all we really need is the guid, the other infos are here for information
    ///         and debugging purposes.
    ///     </para>
    /// </remarks>
    /// <returns></returns>
    public string GetLocalIdentity();
}
