using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Events
{
    /// <summary>
    /// Represents event data for the Published event.
    /// </summary>
    public class ContentPublishedEventArgs : PublishEventArgs<IContent>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentPublishedEventArgs"/> class.
        /// </summary>
        public ContentPublishedEventArgs(IEnumerable<IContent> eventObject, bool canCancel, EventMessages eventMessages)
            : base(eventObject, canCancel, eventMessages)
        { }

        /// <summary>
        /// Determines whether a culture has been published, during a Published event.
        /// </summary>
        public bool HasPublishedCulture(IContent content, string culture)
            => content.WasPropertyDirty(ContentBase.ChangeTrackingPrefix.ChangedCulture + culture);

        /// <summary>
        /// Determines whether a culture has been unpublished, during a Published event.
        /// </summary>
        public bool HasUnpublishedCulture(IContent content, string culture)
            => content.WasPropertyDirty(ContentBase.ChangeTrackingPrefix.UnpublishedCulture + culture); 
    }
}
