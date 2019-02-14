using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Events
{
    /// <summary>
    /// Represents event data for the Publishing event.
    /// </summary>
    public class ContentPublishingEventArgs : PublishEventArgs<IContent>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentPublishingEventArgs"/> class.
        /// </summary>
        public ContentPublishingEventArgs(IEnumerable<IContent> eventObject, EventMessages eventMessages)
            : base(eventObject, eventMessages)
        { }

        /// <summary>
        /// Determines whether a culture is being published, during a Publishing event.
        /// </summary>
        public bool IsPublishingCulture(IContent content, string culture)
            => content.PublishCultureInfos.TryGetValue(culture, out var cultureInfo) && cultureInfo.IsDirty();

        /// <summary>
        /// Determines whether a culture is being unpublished, during a Publishing event.
        /// </summary>
        public bool IsUnpublishingCulture(IContent content, string culture)
            => content.IsPropertyDirty(ContentBase.ChangeTrackingPrefix.UnpublishedCulture + culture); //bit of a hack since we know that the content implementation tracks changes this way
    }
}
