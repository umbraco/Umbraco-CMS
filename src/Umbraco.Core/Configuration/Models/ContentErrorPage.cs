// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Configuration.Models.Validation;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration for a content error page.
/// </summary>
public class ContentErrorPage : ValidatableEntryBase
{
    /// <summary>
    ///     Gets or sets a value for the content Id.
    /// </summary>
    public int ContentId { get; set; }

    /// <summary>
    ///     Gets or sets a value for the content key.
    /// </summary>
    public Guid ContentKey { get; set; }

    /// <summary>
    ///     Gets or sets a value for the content XPath.
    /// </summary>
    public string? ContentXPath { get; set; }

    /// <summary>
    ///     Gets a value indicating whether the <see cref="ContentId" /> field is populated.
    /// </summary>
    public bool HasContentId => ContentId != 0;

    /// <summary>
    ///     Gets a value indicating whether the <see cref="ContentKey" /> field is populated.
    /// </summary>
    public bool HasContentKey => ContentKey != Guid.Empty;

    /// <summary>
    ///     Gets a value indicating whether the <see cref="ContentXPath" /> field is populated.
    /// </summary>
    public bool HasContentXPath => !string.IsNullOrEmpty(ContentXPath);

    /// <summary>
    ///     Gets or sets a value for the content culture.
    /// </summary>
    [Required]
    public string Culture { get; set; } = null!;

    internal override bool IsValid() =>
        base.IsValid() &&
        (HasContentId ? 1 : 0) + (HasContentKey ? 1 : 0) + (HasContentXPath ? 1 : 0) == 1;
}
