namespace Umbraco.Cms.Core.Factories;

/// <summary>
///     Returns the machine identifier from the <c>WEBSITE_INSTANCE_ID</c> environment variable set by Azure App Service.
/// </summary>
/// <remarks>
///     <para>
///         <c>WEBSITE_INSTANCE_ID</c> identifies the instance slot and remains stable across container recycles,
///         unlike <c>Environment.MachineName</c> which changes on every recycle on Azure App Service Linux.
///     </para>
///     <para>Returns <c>null</c> when the environment variable is not present (i.e. outside Azure App Service).</para>
/// </remarks>
public class AzureWebsiteInstanceIdMachineIdentityProvider : IMachineIdentityProvider
{
    /// <summary>
    ///     The name of the Azure App Service environment variable that identifies the instance slot.
    /// </summary>
    public const string WebsiteInstanceIdEnvironmentVariable = "WEBSITE_INSTANCE_ID";

    /// <inheritdoc />
    public string? GetMachineIdentifier()
    {
        var instanceId = Environment.GetEnvironmentVariable(WebsiteInstanceIdEnvironmentVariable);
        return string.IsNullOrWhiteSpace(instanceId) ? null : instanceId;
    }
}
