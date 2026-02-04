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
    #region RTE Anchor values

    private static readonly Regex AnchorRegex = new(@"<a id=\\*""(.*?)\\*"">", RegexOptions.Compiled);
    private static readonly string[] _propertyTypesWithRte = new[] { Constants.PropertyEditors.Aliases.RichText, Constants.PropertyEditors.Aliases.BlockList, Constants.PropertyEditors.Aliases.BlockGrid };

    /// <summary>
    /// Gets content items by their UDI identifiers.
    /// </summary>
    /// <param name="contentService">The content service.</param>
    /// <param name="ids">The UDI identifiers of the content items to retrieve.</param>
    /// <returns>A collection of content items matching the specified UDIs.</returns>
    /// <exception cref="InvalidOperationException">Thrown when any UDI is not a <see cref="GuidUdi"/>.</exception>
    public static IEnumerable<IContent> GetByIds(this IContentService contentService, IEnumerable<Udi> ids)
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

        return contentService.GetByIds(guids.Select(x => x.Guid));
    }

    /// <summary>
    ///     Method to create an IContent object based on the Udi of a parent
    /// </summary>
    /// <param name="contentService"></param>
    /// <param name="name"></param>
    /// <param name="parentId"></param>
    /// <param name="contentTypeAlias"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public static IContent CreateContent(this IContentService contentService, string name, Udi parentId, string contentTypeAlias, int userId = Constants.Security.SuperUserId)
    {
        if (parentId is not GuidUdi guidUdi)
        {
            throw new InvalidOperationException("The UDI provided isn't of type " + typeof(GuidUdi) +
                                                " which is required by content");
        }

        IContent? parent = contentService.GetById(guidUdi.Guid);
        return contentService.Create(name, parent, contentTypeAlias, userId);
    }

    /// <summary>
    ///     Remove all permissions for this user for all nodes
    /// </summary>
    /// <param name="contentService"></param>
    /// <param name="contentId"></param>
    public static void RemoveContentPermissions(this IContentService contentService, int contentId) =>
        contentService.SetPermissions(new EntityPermissionSet(contentId, new EntityPermissionCollection()));

    /// <summary>
    /// Gets all anchor values from Rich Text Editor properties of a content item.
    /// </summary>
    /// <param name="contentService">The content service.</param>
    /// <param name="id">The content item identifier.</param>
    /// <param name="culture">The culture to use, or "*" for all cultures. Defaults to "*".</param>
    /// <returns>A collection of anchor values found in the RTE properties.</returns>
    public static IEnumerable<string> GetAnchorValuesFromRTEs(this IContentService contentService, int id, string? culture = "*")
    {
        var result = new List<string>();

        culture = culture is not "*" ? culture : null;

        IContent? content = contentService.GetById(id);

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

    #endregion
}
