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

    private static readonly Regex AnchorRegex = new("<a id=\"(.*?)\">", RegexOptions.Compiled);

    public static IEnumerable<IContent>? GetByIds(this IContentService contentService, IEnumerable<Udi> ids)
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

    public static IEnumerable<string> GetAnchorValuesFromRTEs(this IContentService contentService, int id, string? culture = "*")
    {
        var result = new List<string>();
        IContent? content = contentService.GetById(id);

        if (content is not null)
        {
            foreach (IProperty contentProperty in content.Properties)
            {
                if (contentProperty.PropertyType.PropertyEditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases
                        .TinyMce))
                {
                    var value = contentProperty.GetValue(culture)?.ToString();
                    if (!string.IsNullOrEmpty(value))
                    {
                        result.AddRange(contentService.GetAnchorValuesFromRTEContent(value));
                    }
                }
            }
        }

        return result;
    }

    public static IEnumerable<string> GetAnchorValuesFromRTEContent(
        this IContentService contentService,
        string rteContent)
    {
        var result = new List<string>();
        MatchCollection matches = AnchorRegex.Matches(rteContent);
        foreach (Match match in matches)
        {
            result.Add(match.Value.Split(Constants.CharArrays.DoubleQuote)[1]);
        }

        return result;
    }

    #endregion
}
