using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Events
{
    public class ContentSavedEventArgs : SaveEventArgs<IContent>
    {
        #region Constructors

        /// <summary>
        /// Creates a new <see cref="ContentSavedEventArgs"/>
        /// </summary>
        /// <param name="eventObject"></param>
        /// <param name="messages"></param>
        /// <param name="additionalData"></param>
        public ContentSavedEventArgs(IEnumerable<IContent> eventObject, EventMessages messages, IDictionary<string, object> additionalData)
            : base(eventObject, false, messages, additionalData)
        {
        }

        #endregion

        /// <summary>
        /// Determines whether a culture has been saved, during a Saved event.
        /// </summary>
        public bool HasSavedCulture(IContent content, string culture)
            => content.WasPropertyDirty("_updatedCulture_" + culture);
    }
}
