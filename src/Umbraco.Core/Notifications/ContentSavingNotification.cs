// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
/// A notification that is used to trigger the IContentService when the Save method is called in the API.
/// </summary>
public sealed class ContentSavingNotification : SavingNotification<IContent>
{
    /// <summary>
    ///  Initializes a new instance of the  <see cref="ContentSavingNotification"/>
    /// </summary>
    public ContentSavingNotification(IContent target, EventMessages messages)
        : base(target, messages)
    {
    }
    /// <summary>
    /// Gets a enumeration of <see cref="IContent"/>.
    /// </summary>
    public ContentSavingNotification(IEnumerable<IContent> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
