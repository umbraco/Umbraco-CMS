using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Api.Management.Mapping.Content;

internal static class ContentStateHelper
{
    internal static ContentState GetContentState(IContent content, string? culture)
        => GetContentState(
            content,
            culture,
            content.Edited,
            content.Published,
            content.AvailableCultures,
            content.EditedCultures ?? Enumerable.Empty<string>(),
            content.PublishedCultures);

    internal static ContentState GetContentState(IDocumentEntitySlim content, string? culture)
        => GetContentState(
            content,
            culture,
            content.Edited,
            content.Published,
            content.CultureNames.Keys,
            content.EditedCultures,
            content.PublishedCultures);

    private static ContentState GetContentState(IEntity entity, string? culture, bool edited, bool published, IEnumerable<string> availableCultures, IEnumerable<string> editedCultures, IEnumerable<string> publishedCultures)
    {
        if (entity.Id <= 0 || (culture is not null && availableCultures.Contains(culture) is false))
        {
            return ContentState.NotCreated;
        }

        var isDraft = published is false ||
                      (culture != null && publishedCultures.Contains(culture) is false);
        if (isDraft)
        {
            return ContentState.Draft;
        }

        var isEdited = culture != null
            ? editedCultures.Contains(culture)
            : edited;

        return isEdited ? ContentState.PublishedPendingChanges : ContentState.Published;
    }
}
