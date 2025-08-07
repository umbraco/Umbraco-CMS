namespace Umbraco.Cms.Core.Factories;

/// <summary>
/// Fetches information of the host machine
/// </summary>
public interface IMachineInfoFactory
{
    /// <summary>
    /// Fetches the name of the Host Machine for identification
    /// </summary>
    /// <returns>A name of the host machine.</returns>
    public string GetMachineName();
}
