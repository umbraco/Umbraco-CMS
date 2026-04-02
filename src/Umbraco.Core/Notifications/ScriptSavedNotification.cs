// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after a script file has been saved.
/// </summary>
/// <remarks>
///     This notification is published by the <see cref="Services.IFileService"/> after the script has been persisted.
///     It is not cancelable since the save operation has already completed.
/// </remarks>
public class ScriptSavedNotification : SavedNotification<IScript>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ScriptSavedNotification"/> class with a single script.
    /// </summary>
    /// <param name="target">The script that was saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public ScriptSavedNotification(IScript target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ScriptSavedNotification"/> class with multiple scripts.
    /// </summary>
    /// <param name="target">The collection of scripts that were saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public ScriptSavedNotification(IEnumerable<IScript> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
