// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published before a script file is saved.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the save operation.
///     The notification is published by the <see cref="Services.IFileService"/> before the script is persisted.
/// </remarks>
public class ScriptSavingNotification : SavingNotification<IScript>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ScriptSavingNotification"/> class with a single script.
    /// </summary>
    /// <param name="target">The script being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public ScriptSavingNotification(IScript target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ScriptSavingNotification"/> class with multiple scripts.
    /// </summary>
    /// <param name="target">The collection of scripts being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public ScriptSavingNotification(IEnumerable<IScript> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
