using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Events
{
    public class ContentPublishedEventArgs : PublishEventArgs<IContent>
    {
        public ContentPublishedEventArgs(IEnumerable<IContent> eventObject, bool canCancel, EventMessages eventMessages)
            : base(eventObject, canCancel, eventMessages)
        {
        }

        /// <summary>
        /// Determines whether a culture has been published, during a Published event.
        /// </summary>
        public bool HasPublishedCulture(IContent content, string culture)
            => content.WasPropertyDirty("_changedCulture_" + culture);

        /// <summary>
        /// Determines whether a culture has been unpublished, during a Published event.
        /// </summary>
        public bool HasUnpublishedCulture(IContent content, string culture)
            => content.WasPropertyDirty("_unpublishedCulture_" + culture); 



    }
}
