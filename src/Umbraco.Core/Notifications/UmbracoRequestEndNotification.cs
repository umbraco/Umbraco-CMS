// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification raised on each request end.
/// </summary>
public class UmbracoRequestEndNotification : INotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoRequestEndNotification" /> class.
    /// </summary>
    public UmbracoRequestEndNotification(IUmbracoContext umbracoContext) => UmbracoContext = umbracoContext;

    /// <summary>
    ///     Gets the <see cref="IUmbracoContext" />
    /// </summary>
    public IUmbracoContext UmbracoContext { get; }
}
