namespace Umbraco.Cms.Core.Factories;

internal sealed class MachineInfoFactory : IMachineInfoFactory
{

    /// <inheritdoc />
    public string GetMachineIdentifier() => Environment.MachineName;
}
