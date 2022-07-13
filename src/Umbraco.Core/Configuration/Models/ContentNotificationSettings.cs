// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for content notification settings.
/// </summary>
public class ContentNotificationSettings
{
    internal const bool StaticDisableHtmlEmail = false;

    /// <summary>
    ///     Gets or sets a value for the email address for notifications.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether HTML email notifications should be disabled.
    /// </summary>
    [DefaultValue(StaticDisableHtmlEmail)]
    public bool DisableHtmlEmail { get; set; } = StaticDisableHtmlEmail;
}
