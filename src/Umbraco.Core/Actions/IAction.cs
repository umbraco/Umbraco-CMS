// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Actions;

/// <summary>
///     Defines a back office action that can be permission assigned or subscribed to for notifications
/// </summary>
/// <remarks>
///     If an IAction returns false for both ShowInNotifier and CanBePermissionAssigned then the IAction should not exist
/// </remarks>
public interface IAction : IDiscoverable
{
    /// <inheritdoc cref="Letter"/>
    const string ActionLetter = "";

    /// <inheritdoc cref="Alias"/>
    const string ActionAlias = "";

    /// <summary>
    ///     Gets the letter used to assign a permission (must be unique).
    /// </summary>
    string Letter { get; }

    /// <summary>
    ///     Gets a value indicating whether whether to allow subscribing to notifications for this action
    /// </summary>
    bool ShowInNotifier { get; }

    /// <summary>
    ///     Gets a value indicating whether whether to allow assigning permissions based on this action
    /// </summary>
    bool CanBePermissionAssigned { get; }

    /// <summary>
    ///     Gets the icon to display for this action
    /// </summary>
    string Icon { get; }

    /// <summary>
    ///     Gets the alias for this action (must be unique).
    ///     This is all lower-case because of case sensitive filesystems, see issue: https://github.com/umbraco/Umbraco-CMS/issues/11670.
    /// </summary>
    string Alias { get; }

    /// <summary>
    ///     Gets the category used for this action
    /// </summary>
    /// <remarks>
    ///     Used in the UI when assigning permissions
    /// </remarks>
    string? Category { get; }
}
