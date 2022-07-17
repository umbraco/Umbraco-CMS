namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Used to notify when the core runtime can do an unattended install.
/// </summary>
/// <remarks>
///     It is entirely up to the handler to determine if an unattended installation should occur and
///     to perform the logic.
/// </remarks>
public class RuntimeUnattendedInstallNotification : INotification
{
}
