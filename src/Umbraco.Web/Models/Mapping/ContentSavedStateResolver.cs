using System;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{

    /// <summary>
    /// Returns the <see cref="ContentSavedState?"/> for an <see cref="IContent"/> item
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ContentBasicSavedStateResolver<T> : IValueResolver<IContent, IContentProperties<T>, ContentSavedState?>
        where T : ContentPropertyBasic
    {
        private readonly ContentSavedStateResolver<T> _inner = new ContentSavedStateResolver<T>();

        public ContentSavedState? Resolve(IContent source, IContentProperties<T> destination, ContentSavedState? destMember, ResolutionContext context)
        {
            return _inner.Resolve(source, destination, default, context);
        }
    }

    /// <summary>
    /// Returns the <see cref="ContentSavedState"/> for an <see cref="IContent"/> item
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ContentSavedStateResolver<T> : IValueResolver<IContent, IContentProperties<T>, ContentSavedState>
        where T : ContentPropertyBasic
    {
        public ContentSavedState Resolve(IContent source, IContentProperties<T> destination, ContentSavedState destMember, ResolutionContext context)
        {
            PublishedState publishedState;
            bool isEdited;
            bool isCreated;

            if (source.ContentType.VariesByCulture())
            {
                //Get the culture from the context which will be set during the mapping operation for each variant
                var culture = context.Options.GetCulture();

                //a culture needs to be in the context for a variant content item
                if (culture == null)
                    throw new InvalidOperationException($"No culture found in mapping operation when one is required for a culture variant");

                publishedState = source.PublishedState == PublishedState.Unpublished //if the entire document is unpublished, then flag every variant as unpublished
                    ? PublishedState.Unpublished
                    : source.IsCulturePublished(culture)
                        ? PublishedState.Published
                        : PublishedState.Unpublished;

                isEdited = source.IsCultureEdited(culture);
                isCreated = source.Id > 0 && source.IsCultureAvailable(culture);
            }
            else
            {
                publishedState = source.PublishedState == PublishedState.Unpublished
                    ? PublishedState.Unpublished
                    : PublishedState.Published;

                isEdited = source.Edited;
                isCreated = source.Id > 0;
            }

            if (!isCreated)
                return ContentSavedState.NotCreated;

            if (publishedState == PublishedState.Unpublished)
                return ContentSavedState.Draft;

            if (publishedState == PublishedState.Published)
                return isEdited ? ContentSavedState.PublishedPendingChanges : ContentSavedState.Published;

            throw new NotSupportedException($"PublishedState {publishedState} is not supported.");
        }
    }
}
