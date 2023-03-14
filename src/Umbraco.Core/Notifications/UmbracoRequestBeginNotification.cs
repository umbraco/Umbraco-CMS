// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification raised on each request begin.
/// </summary>
public class UmbracoRequestBeginNotification : INotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoRequestBeginNotification" /> class.
    /// </summary>
    public UmbracoRequestBeginNotification(IUmbracoContext umbracoContext) => UmbracoContext = umbracoContext;

    /// <summary>
    ///     Gets the <see cref="IUmbracoContext" />
    /// </summary>
    public IUmbracoContext UmbracoContext { get; }
}
