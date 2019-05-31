using Umbraco.Core.Composing;

namespace Umbraco.Web.Actions
{
    /// <summary>
    /// Defines a back office action that can be permission assigned or subscribed to for notifications
    /// </summary>
    public interface IAction : IDiscoverable
    {
        /// <summary>
        /// The letter used to assign a permission (must be unique)
        /// </summary>
        char Letter { get; }

        /// <summary>
        /// Whether to allow subscribing to notifications for this action
        /// </summary>
        bool ShowInNotifier { get; }

        /// <summary>
        /// Whether to allow assigning permissions based on this action
        /// </summary>
        bool CanBePermissionAssigned { get; }

        /// <summary>
        /// The icon to display for this action
        /// </summary>
        string Icon { get; }

        /// <summary>
        /// The alias for this action (must be unique)
        /// </summary>
        string Alias { get; }

        /// <summary>
        /// The category used for this action
        /// </summary>
        /// <remarks>
        /// Used in the UI when assigning permissions
        /// </remarks>
        string Category { get; }
    }
}
