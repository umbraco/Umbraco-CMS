using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Sync;

/// <summary>
///     Gets the current server's <see cref="ServerRole" /> based on active servers registered with
///     <see cref="IServerRegistrationService" />
/// </summary>
/// <remarks>
///     This is the default service which determines a server's role by using a master election process.
///     The scheduling publisher election process doesn't occur until just after startup so this election process doesn't
///     really affect the primary startup phase.
/// </remarks>
public sealed class ElectedServerRoleAccessor : IServerRoleAccessor
{
    private readonly IServerRegistrationService _registrationService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ElectedServerRoleAccessor" /> class.
    /// </summary>
    /// <param name="registrationService">The registration service.</param>
    /// <param name="options">Some options.</param>
    public ElectedServerRoleAccessor(IServerRegistrationService registrationService) => _registrationService =
        registrationService ?? throw new ArgumentNullException(nameof(registrationService));

    /// <summary>
    ///     Gets the role of the current server in the application environment.
    /// </summary>
    public ServerRole CurrentServerRole => _registrationService.GetCurrentServerRole();
}
