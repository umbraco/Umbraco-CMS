using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Api.Management.Mapping.Content;

internal static class PublishableVariantStateHelper
{
    internal static PublishableVariantState GetState(IPublishableContentBase content, string? culture)
        => GetState(
            content,
            culture,
            content.Edited,
            content.Published,
            content.Trashed,
            content.AvailableCultures,
            content.EditedCultures ?? Enumerable.Empty<string>(),
            content.PublishedCultures);

    internal static PublishableVariantState GetState(IPublishableContentEntitySlim entity, string? culture)
        => GetState(
            entity,
            culture,
            entity.Edited,
            entity.Published,
            entity.Trashed,
            entity.CultureNames.Keys,
            entity.EditedCultures,
            entity.PublishedCultures);

    internal static PublishableVariantState GetState(IDocumentEntitySlim content, string? culture)
        => GetState((IPublishableContentEntitySlim)content, culture);

    internal static PublishableVariantState GetState(IElementEntitySlim element, string? culture)
        => GetState((IPublishableContentEntitySlim)element, culture);

    private static PublishableVariantState GetState(IEntity entity, string? culture, bool edited, bool published, bool trashed, IEnumerable<string> availableCultures, IEnumerable<string> editedCultures, IEnumerable<string> publishedCultures)
    {
        if (entity.Id <= 0 || (culture is not null && availableCultures.Contains(culture) is false))
        {
            return PublishableVariantState.NotCreated;
        }

        if (trashed)
        {
            return PublishableVariantState.Trashed;
        }

        var isDraft = published is false ||
                      (culture != null && publishedCultures.Contains(culture) is false);
        if (isDraft)
        {
            return PublishableVariantState.Draft;
        }

        var isEdited = culture != null
            ? editedCultures.Contains(culture)
            : edited;

        return isEdited ? PublishableVariantState.PublishedPendingChanges : PublishableVariantState.Published;
    }
}
