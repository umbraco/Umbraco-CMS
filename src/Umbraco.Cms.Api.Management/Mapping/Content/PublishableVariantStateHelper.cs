using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Mapping.Content;

internal static class PublishableVariantStateHelper
{
    /// <summary>
    ///     Gets the "worst" publish state across all variants of an entity (Draft is worse than PublishedPendingChanges,
    ///     which is worse than Trashed, which is worse than Published), for callers that need a single state to
    ///     represent a possibly-variant entity without picking a specific culture.
    /// </summary>
    internal static PublishableVariantState GetAggregateState(IPublishableContentEntitySlim entity)
    {
        if (entity.Variations.VariesByCulture() is false || entity.CultureNames.Count == 0)
        {
            return GetState(entity, null);
        }

        PublishableVariantState? worst = null;
        foreach (var culture in entity.CultureNames.Keys)
        {
            PublishableVariantState state = GetState(entity, culture);
            if (worst is null || IsWorseThan(state, worst.Value))
            {
                worst = state;
            }
        }

        return worst ?? PublishableVariantState.NotCreated;
    }

    // Lower rank = worse state; used by GetAggregateState to pick the single most-attention-needing variant.
    private static bool IsWorseThan(PublishableVariantState state, PublishableVariantState other)
        => Rank(state) < Rank(other);

    private static int Rank(PublishableVariantState state) => state switch
    {
        PublishableVariantState.Draft => 0,
        PublishableVariantState.PublishedPendingChanges => 1,
        PublishableVariantState.Trashed => 2,
        PublishableVariantState.Published => 3,
        PublishableVariantState.NotCreated => 4,
        _ => 4,
    };

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
