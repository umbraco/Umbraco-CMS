namespace Umbraco.Cms.Core.Sync;

/// <summary>
/// Provides server role access for single-server deployments.
/// </summary>
/// <remarks>
/// <para>
/// Can be used when Umbraco is definitely not operating in a load balanced scenario
/// to micro-optimize some startup performance.
/// </para>
/// <para>
/// The micro optimization is specifically to avoid a DB query just after the app starts up
/// to determine the <see cref="ServerRole"/>, which by default is done with scheduling
/// publisher election by a database query. The master election process doesn't occur until
/// just after startup so this micro optimization doesn't really affect the primary startup phase.
/// </para>
/// </remarks>
public class SingleServerRoleAccessor : IServerRoleAccessor
{
    /// <inheritdoc />
    public ServerRole CurrentServerRole => ServerRole.Single;
}
