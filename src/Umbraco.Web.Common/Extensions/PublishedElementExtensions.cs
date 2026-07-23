// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods for <c>IPublishedElement</c>.
/// </summary>
public static class PublishedElementExtensions
{
    #region Creator/Writer Names

    /// <summary>
    ///     Gets the name of the content item creator.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="userService">The user service.</param>
    /// <returns>The name of the creator, or null if not found.</returns>
    public static string? CreatorName(this IPublishedElement content, IUserService userService) =>
        userService.GetProfileById(content.CreatorId)?.Name;

    /// <summary>
    ///     Gets the name of the content item writer.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="userService">The user service.</param>
    /// <returns>The name of the writer, or null if not found.</returns>
    public static string? WriterName(this IPublishedElement content, IUserService userService) =>
        userService.GetProfileById(content.WriterId)?.Name;

    #endregion
}
