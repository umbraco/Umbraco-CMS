using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Api.Management.Mapping.Content;

// TODO ELEMENTS: rename this to VariantStateHelper or ContentVariantStateHelper (depending on the new name for DocumentVariantState)
internal static class DocumentVariantStateHelper
{
    internal static DocumentVariantState GetState(IPublishableContentBase content, string? culture)
        => GetState(
            content,
            culture,
            content.Edited,
            content.Published,
            content.Trashed,
            content.AvailableCultures,
            content.EditedCultures ?? Enumerable.Empty<string>(),
            content.PublishedCultures);

    internal static DocumentVariantState GetState(IDocumentEntitySlim content, string? culture)
        => GetState(
            content,
            culture,
            content.Edited,
            content.Published,
            content.Trashed,
            content.CultureNames.Keys,
            content.EditedCultures,
            content.PublishedCultures);

    internal static DocumentVariantState GetState(IElementEntitySlim element, string? culture)
        => GetState(
            element,
            culture,
            element.Edited,
            element.Published,
            element.Trashed,
            element.CultureNames.Keys,
            element.EditedCultures,
            element.PublishedCultures);

    private static DocumentVariantState GetState(IEntity entity, string? culture, bool edited, bool published, bool trashed, IEnumerable<string> availableCultures, IEnumerable<string> editedCultures, IEnumerable<string> publishedCultures)
    {
        if (entity.Id <= 0 || (culture is not null && availableCultures.Contains(culture) is false))
        {
            return DocumentVariantState.NotCreated;
        }

        if (trashed)
        {
            return DocumentVariantState.Trashed;
        }

        var isDraft = published is false ||
                      (culture != null && publishedCultures.Contains(culture) is false);
        if (isDraft)
        {
            return DocumentVariantState.Draft;
        }

        var isEdited = culture != null
            ? editedCultures.Contains(culture)
            : edited;

        return isEdited ? DocumentVariantState.PublishedPendingChanges : DocumentVariantState.Published;
    }
}
