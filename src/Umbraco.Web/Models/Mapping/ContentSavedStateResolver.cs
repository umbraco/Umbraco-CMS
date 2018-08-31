using System;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class ContentSavedStateResolver : IValueResolver<IContent, ContentVariantDisplay, ContentSavedState>
    {
        public ContentSavedState Resolve(IContent source, ContentVariantDisplay destination, ContentSavedState destMember, ResolutionContext context)
        {
            PublishedState publishedState;
            bool isEdited;

            if (source.ContentType.VariesByCulture())
            {
                //Get the culture from the context which will be set during the mapping operation for each variant
                var culture = context.GetCulture();

                //a culture needs to be in the context for a variant content item
                if (culture == null)
                    throw new InvalidOperationException($"No culture found in mapping operation when one is required for a culture variant");

                publishedState = source.PublishedState == PublishedState.Unpublished //if the entire document is unpublished, then flag every variant as unpublished
                    ? PublishedState.Unpublished
                    : source.IsCulturePublished(culture)
                        ? PublishedState.Published
                        : PublishedState.Unpublished;

                isEdited = source.IsCultureEdited(culture);
            }
            else
            {
                publishedState = source.PublishedState == PublishedState.Unpublished
                    ? PublishedState.Unpublished
                    : PublishedState.Published;

                isEdited = source.Edited;
            }

            if (publishedState == PublishedState.Unpublished)
                return isEdited && source.Id > 0 ? ContentSavedState.Draft : ContentSavedState.NotCreated;

            if (publishedState == PublishedState.Published)
                return isEdited ? ContentSavedState.PublishedPendingChanges : ContentSavedState.Published;

            throw new NotSupportedException($"PublishedState {publishedState} is not supported.");
        }
    }
}
