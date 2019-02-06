using System;
using System.Linq;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Events
{
    public class ContentPublishingEventArgs : PublishEventArgs<IContent>
    {
        /// <summary>
        /// Creates a new <see cref="ContentPublishingEventArgs"/>
        /// </summary>
        /// <param name="eventObject"></param>
        /// <param name="eventMessages"></param>
        public ContentPublishingEventArgs(IEnumerable<IContent> eventObject, EventMessages eventMessages)
            : base(eventObject, eventMessages)
        {
        }

        /// <summary>
        /// Determines whether a culture is being published, during a Publishing event.
        /// </summary>
        public bool IsPublishingCulture(IContent content, string culture)
            => content.PublishCultureInfos.TryGetValue(culture, out var cultureInfo) && cultureInfo.IsDirty();

        /// <summary>
        /// Determines whether a culture is being unpublished, during a Publishing event.
        /// </summary>
        public bool IsUnpublishingCulture(IContent content, string culture)
            => content.IsPropertyDirty("_unpublishedCulture_" + culture); //bit of a hack since we know that the content implementation tracks changes this way

    }
}
