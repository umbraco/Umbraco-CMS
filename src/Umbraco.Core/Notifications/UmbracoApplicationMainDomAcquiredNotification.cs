// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

// TODO (V10): Remove this class.

/// <summary>
///     Notification that occurs during Umbraco boot after the MainDom has been acquired.
/// </summary>
[Obsolete(
    "This notification was added to the core runtime start-up as a hook for Umbraco Cloud local connection string and database setup. " +
    "Following re-work they are no longer used (from Deploy 9.2.0)." +
    "Given they are non-documented and no other use is expected, they can be removed in the next major release")]
public class UmbracoApplicationMainDomAcquiredNotification : INotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoApplicationMainDomAcquiredNotification" /> class.
    /// </summary>
    public UmbracoApplicationMainDomAcquiredNotification()
    {
    }
}
