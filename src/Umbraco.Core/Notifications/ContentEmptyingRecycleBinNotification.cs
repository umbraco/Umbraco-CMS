// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
/// A notification that is used to trigger the IContentService when the EmptyRecycleBin method is called in the API.
/// </summary>
public sealed class ContentEmptyingRecycleBinNotification : EmptyingRecycleBinNotification<IContent>
{
    /// <summary>
    ///  Initializes a new instance of the  <see cref="ContentEmptyingRecycleBinNotification"/>
    /// </summary>
    /// <param name="deletedEntities">
    ///  The collection of IContent objects being deleted.
    /// </param>
    /// <param name="messages">
    /// Initializes a new instance of the <see cref="EventMessages"/>.
    /// </param>
    public ContentEmptyingRecycleBinNotification(IEnumerable<IContent>? deletedEntities, EventMessages messages)
        : base(
        deletedEntities, messages)
    {
    }
}
