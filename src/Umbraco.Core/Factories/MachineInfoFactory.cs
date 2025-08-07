namespace Umbraco.Cms.Core.Factories;

public class MachineInfoFactory : IMachineInfoFactory
{
    /// <summary>
    /// Fetches the name of the host machine from the system environment.
    /// </summary>
    /// <returns>The name of the host machine.</returns>
    public string GetMachineName() => Environment.MachineName;
}
