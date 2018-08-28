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

                //it can only be 'edited' if the content item is persisted and if the variant has a name and it's flagged as edited
                isEdited = source.Id > 0 && source.IsCultureAvailable(culture) && source.IsCultureEdited(culture);
            }
            else
            {
                publishedState = source.PublishedState == PublishedState.Unpublished
                    ? PublishedState.Unpublished
                    : PublishedState.Published;

                isEdited = source.Id > 0 && source.Edited;
            }

            //now we can calculate the content state
            if (!isEdited && publishedState == PublishedState.Unpublished)
                return ContentSavedState.NotCreated;
            if (isEdited && publishedState == PublishedState.Unpublished)
                return ContentSavedState.Draft;
            if (!isEdited && publishedState == PublishedState.Published)
                return ContentSavedState.Published;
            return ContentSavedState.PublishedPendingChanges;
        }
    }
}
