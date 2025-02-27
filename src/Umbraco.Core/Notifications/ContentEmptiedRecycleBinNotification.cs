// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
/// A notification that is used to trigger the IContentService when the EmptyRecycleBin method is called in the API, after the RecycleBin has been deleted.
/// </summary>
public sealed class ContentEmptiedRecycleBinNotification : EmptiedRecycleBinNotification<IContent>
{
    /// <summary>
    ///  Initializes a new instance of the  <see cref="ContentEmptyingRecycleBinNotification"/>
    /// </summary>
    /// <param name="deletedEntities">
    ///  The collection of deleted IContent object.
    /// </param>
    /// <param name="messages">
    /// Initializes a new instance of the <see cref="EventMessages"/>.
    /// </param>
    public ContentEmptiedRecycleBinNotification(IEnumerable<IContent> deletedEntities, EventMessages messages)
        : base(
        deletedEntities, messages)
    {
    }
}
