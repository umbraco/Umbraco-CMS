// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the IFileService when the DeleteScript method is called in the API.
/// </summary>
public class ScriptDeletingNotification : DeletingNotification<IScript>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ScriptDeletingNotification"/> class
    ///     with a single script.
    /// </summary>
    /// <param name="target">The script being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public ScriptDeletingNotification(IScript target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ScriptDeletingNotification"/> class
    ///     with multiple scripts.
    /// </summary>
    /// <param name="target">The scripts being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public ScriptDeletingNotification(IEnumerable<IScript> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
