using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models.Mapping;

/// <summary>
///     Returns the <see cref="ContentSavedState" /> for an <see cref="IContent" /> item
/// </summary>
/// <typeparam name="T"></typeparam>
public class ContentBasicSavedStateMapper<T>
    where T : ContentPropertyBasic
{
    private readonly ContentSavedStateMapper<T> _inner = new();

    public ContentSavedState? Map(IContent source, MapperContext context) => _inner.Map(source, context);
}

/// <summary>
///     Returns the <see cref="ContentSavedState" /> for an <see cref="IContent" /> item
/// </summary>
/// <typeparam name="T"></typeparam>
public class ContentSavedStateMapper<T>
    where T : ContentPropertyBasic
{
    public ContentSavedState Map(IContent source, MapperContext context)
    {
        PublishedState publishedState;
        bool isEdited;
        bool isCreated;

        if (source.ContentType.VariesByCulture())
        {
            // Get the culture from the context which will be set during the mapping operation for each variant
            var culture = context.GetCulture();

            // a culture needs to be in the context for a variant content item
            if (culture == null)
            {
                throw new InvalidOperationException(
                    "No culture found in mapping operation when one is required for a culture variant");
            }

            publishedState =
                source.PublishedState ==
                PublishedState
                    .Unpublished // if the entire document is unpublished, then flag every variant as unpublished
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
        {
            return ContentSavedState.NotCreated;
        }

        if (publishedState == PublishedState.Unpublished)
        {
            return ContentSavedState.Draft;
        }

        if (publishedState == PublishedState.Published)
        {
            return isEdited ? ContentSavedState.PublishedPendingChanges : ContentSavedState.Published;
        }

        throw new NotSupportedException($"PublishedState {publishedState} is not supported.");
    }
}
