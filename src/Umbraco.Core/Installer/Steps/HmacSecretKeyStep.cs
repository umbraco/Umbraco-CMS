using Umbraco.Cms.Core.Models.Installer;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Core.Installer.Steps;

/// <summary>
/// An installation step that creates a unique HMAC secret key for imaging URL authentication.
/// </summary>
public class HmacSecretKeyStep : StepBase, IInstallStep
{
    private readonly IHmacSecretKeyService _hmacSecretKeyService;

    /// <summary>
    /// Initializes a new instance of the <see cref="HmacSecretKeyStep"/> class.
    /// </summary>
    /// <param name="hmacSecretKeyService">The service used to create HMAC secret keys.</param>
    public HmacSecretKeyStep(IHmacSecretKeyService hmacSecretKeyService)
        => _hmacSecretKeyService = hmacSecretKeyService;

    /// <inheritdoc />
    public async Task<Attempt<InstallationResult>> ExecuteAsync(InstallData _)
    {
        Attempt<HmacSecretKeyOperationStatus> result = await _hmacSecretKeyService.CreateHmacSecretKeyAsync();

        return result.Result is HmacSecretKeyOperationStatus.Error
            ? FailWithMessage("Failed to generate and persist the imaging HMAC secret key. Check the logs for details.")
            : Success();
    }

    /// <inheritdoc />
    public Task<bool> RequiresExecutionAsync(InstallData _)
        => Task.FromResult(_hmacSecretKeyService.HasHmacSecretKey() is false);
}
