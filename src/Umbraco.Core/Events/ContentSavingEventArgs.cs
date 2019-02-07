using System.Linq;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Events
{
    public class ContentSavingEventArgs : SaveEventArgs<IContent>
    {
        #region Factory Methods
        /// <summary>
        /// Converts <see cref="ContentSavingEventArgs"/> to <see cref="ContentSavedEventArgs"/> while preserving all args state
        /// </summary>
        /// <returns></returns>
        public ContentSavedEventArgs ToContentSavedEventArgs()
        {
            return new ContentSavedEventArgs(EventObject, Messages, AdditionalData)
            {
                EventState = EventState
            };
        }

        /// <summary>
        /// Converts <see cref="ContentSavingEventArgs"/> to <see cref="ContentPublishedEventArgs"/> while preserving all args state
        /// </summary>
        /// <returns></returns>
        public ContentPublishedEventArgs ToContentPublishedEventArgs()
        {
            return new ContentPublishedEventArgs(EventObject, false, Messages)
            {
                EventState = EventState,
                AdditionalData = AdditionalData
            };
        }

        /// <summary>
        /// Converts <see cref="ContentSavingEventArgs"/> to <see cref="ContentPublishingEventArgs"/> while preserving all args state
        /// </summary>
        /// <returns></returns>
        public ContentPublishingEventArgs ToContentPublishingEventArgs()
        {
            return new ContentPublishingEventArgs(EventObject, Messages)
            {
                EventState = EventState,
                AdditionalData = AdditionalData
            };
        } 
        #endregion

        #region Constructors

        public ContentSavingEventArgs(IEnumerable<IContent> eventObject, EventMessages eventMessages) : base(eventObject, eventMessages)
        {
        }

        public ContentSavingEventArgs(IContent eventObject, EventMessages eventMessages) : base(eventObject, eventMessages)
        {
        }

        #endregion

        /// <summary>
        /// Determines whether a culture is being saved, during a Saving event.
        /// </summary>
        public bool IsSavingCulture(IContent content, string culture) => content.CultureInfos.TryGetValue(culture, out var cultureInfo) && cultureInfo.IsDirty();
    }
}
