// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.RegularExpressions;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Extensions;

/// <summary>
///     Content service extension methods
/// </summary>
public static class ContentServiceExtensions
{
    private static readonly Regex AnchorRegex = new(@"<a id=\\*""(.*?)\\*"">", RegexOptions.Compiled);
    private static readonly string[] _propertyTypesWithRte = new[] { Constants.PropertyEditors.Aliases.RichText, Constants.PropertyEditors.Aliases.BlockList, Constants.PropertyEditors.Aliases.BlockGrid };

    /// <summary>
    /// Gets content items by their UDI identifiers.
    /// </summary>
    /// <param name="contentService">The content service.</param>
    /// <param name="ids">The UDI identifiers of the content items to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of content items matching the specified UDIs.</returns>
    /// <exception cref="InvalidOperationException">Thrown when any UDI is not a <see cref="GuidUdi"/>.</exception>
    public static Task<IEnumerable<IContent>> GetByIdsAsync(this IContentService contentService, IEnumerable<Udi> ids, CancellationToken cancellationToken)
    {
        var guids = new List<GuidUdi>();
        foreach (Udi udi in ids)
        {
            if (udi is not GuidUdi guidUdi)
            {
                throw new InvalidOperationException("The UDI provided isn't of type " + typeof(GuidUdi) +
                                                    " which is required by content");
            }

            guids.Add(guidUdi);
        }

        return contentService.GetByIdsAsync(guids.Select(x => x.Guid), cancellationToken);
    }

    /// <summary>
    ///     Method to create an IContent object based on the Udi of a parent
    /// </summary>
    /// <param name="contentService">The content service.</param>
    /// <param name="name">The name of the content item to create.</param>
    /// <param name="parentId">The UDI of the parent to create the content item under.</param>
    /// <param name="contentTypeAlias">The alias of the content type to create.</param>
    /// <param name="userKey">The key of the user creating the content item.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created content item.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the UDI is not a <see cref="GuidUdi"/>.</exception>
    public static async Task<IContent> CreateContentAsync(this IContentService contentService, string name, Udi parentId, string contentTypeAlias, Guid userKey, CancellationToken cancellationToken)
    {
        if (parentId is not GuidUdi guidUdi)
        {
            throw new InvalidOperationException("The UDI provided isn't of type " + typeof(GuidUdi) +
                                                " which is required by content");
        }

        IContent? parent = await contentService.GetByIdAsync(guidUdi.Guid, cancellationToken);
        if (parent is null)
        {
            throw new ArgumentNullException(nameof(parentId), "No content found for the specified parent UDI.");
        }

        return await contentService.CreateAsync(name, parent, contentTypeAlias, userKey, cancellationToken);
    }

    /// <summary>
    ///     Remove all permissions for this user for all nodes
    /// </summary>
    /// <param name="contentService"></param>
    /// <param name="contentId"></param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [Obsolete("Use IUserGroup.GranularPermissions (persisted via IUserGroupService) to manage document permissions instead. Scheduled for removal in Umbraco 21.")]
#pragma warning disable CS0618 // Type or member is obsolete
    public static Task RemoveContentPermissionsAsync(this IContentService contentService, int contentId, CancellationToken cancellationToken) =>
        contentService.SetPermissionsAsync(new EntityPermissionSet(contentId, new EntityPermissionCollection()), cancellationToken);
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>
    /// Gets all anchor values from Rich Text Editor properties of a content item.
    /// </summary>
    /// <param name="contentService">The content service.</param>
    /// <param name="key">The content item key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="culture">The culture to use, or "*" for all cultures. Defaults to "*".</param>
    /// <returns>A collection of anchor values found in the RTE properties.</returns>
    public static async Task<IEnumerable<string>> GetAnchorValuesFromRTEsAsync(this IContentService contentService, Guid key, CancellationToken cancellationToken, string? culture = "*")
    {
        var result = new List<string>();

        culture = culture is not "*" ? culture : null;

        IContent? content = await contentService.GetByIdAsync(key, cancellationToken);
        if (content is null)
        {
            return result;
        }

        foreach (IProperty contentProperty in content.Properties.Where(s => _propertyTypesWithRte.Contains(s.PropertyType.PropertyEditorAlias)))
        {
            var value = contentProperty.GetValue(culture)?.ToString();
            if (!string.IsNullOrEmpty(value))
            {
                result.AddRange(contentService.GetAnchorValuesFromRTEContent(value));
            }
        }

        return result;
    }

    /// <summary>
    /// Extracts anchor values from Rich Text Editor content.
    /// </summary>
    /// <param name="contentService">The content service.</param>
    /// <param name="rteContent">The RTE content to extract anchors from.</param>
    /// <returns>A collection of anchor values found in the content.</returns>
    public static IEnumerable<string> GetAnchorValuesFromRTEContent(
        this IContentService contentService,
        string rteContent)
    {
        var result = new List<string>();
        MatchCollection matches = AnchorRegex.Matches(rteContent);
        foreach (Match match in matches)
        {
            result.Add(match.Groups[1].Value);
        }

        return result;
    }
}
