using System.Collections.Generic;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Events
{
    /// <summary>
    /// Represents event data for the Saved event.
    /// </summary>
    public class ContentSavedEventArgs : SaveEventArgs<IContent>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentSavedEventArgs"/> class.
        /// </summary>
        public ContentSavedEventArgs(IEnumerable<IContent> eventObject, EventMessages messages, IDictionary<string, object> additionalData)
            : base(eventObject, false, messages, additionalData)
        { }

        /// <summary>
        /// Determines whether a culture has been saved, during a Saved event.
        /// </summary>
        public bool HasSavedCulture(IContent content, string culture)
            => content.WasPropertyDirty(ContentBase.ChangeTrackingPrefix.UpdatedCulture + culture);
    }
}
