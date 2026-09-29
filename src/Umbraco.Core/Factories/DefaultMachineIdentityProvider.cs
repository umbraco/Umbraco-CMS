namespace Umbraco.Cms.Core.Factories;

/// <summary>
///     Returns <see cref="Environment.MachineName" /> as the machine identifier.
/// </summary>
/// <remarks>
///     This is the final fallback provider and always returns a non-null value.
/// </remarks>
public class DefaultMachineIdentityProvider : IMachineIdentityProvider
{
    /// <inheritdoc />
    public string? GetMachineIdentifier() => Environment.MachineName;
}
