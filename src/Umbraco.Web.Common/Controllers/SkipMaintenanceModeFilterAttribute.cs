namespace Umbraco.Cms.Web.Common.Controllers;

/// <summary>
/// Suppresses <see cref="MaintenanceModeActionFilterAttribute"/> for the decorated controller or action,
/// allowing it to respond normally during application upgrades.
/// </summary>
/// <remarks>
/// Apply to endpoints that must remain accessible during an upgrade — for example, server status and
/// configuration endpoints that the backoffice needs to detect the upgrading state.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class SkipMaintenanceModeFilterAttribute : Attribute
{
}
