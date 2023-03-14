namespace Umbraco.Cms.Core.Sync;

/// <summary>
///     Can be used when Umbraco is definitely not operating in a Load Balanced scenario to micro-optimize some startup
///     performance
/// </summary>
/// <remarks>
///     The micro optimization is specifically to avoid a DB query just after the app starts up to determine the
///     <see cref="ServerRole" />
///     which by default is done with scheduling publisher election by a database query. The master election process
///     doesn't occur until just after startup
///     so this micro optimization doesn't really affect the primary startup phase.
/// </remarks>
public class SingleServerRoleAccessor : IServerRoleAccessor
{
    public ServerRole CurrentServerRole => ServerRole.Single;
}
