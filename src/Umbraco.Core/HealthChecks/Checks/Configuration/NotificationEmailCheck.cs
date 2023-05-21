// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.HealthChecks.Checks.Configuration;

/// <summary>
///     Health check for the recommended production configuration for Notification Email.
/// </summary>
[HealthCheck(
    "3E2F7B14-4B41-452B-9A30-E67FBC8E1206",
    "Notification Email Settings",
    Description = "If notifications are used, the 'from' email address should be specified and changed from the default value.",
    Group = "Configuration")]
public class NotificationEmailCheck : AbstractSettingsCheck
{
    private const string DefaultFromEmail = "your@email.here";
    private readonly IOptionsMonitor<ContentSettings> _contentSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="NotificationEmailCheck" /> class.
    /// </summary>
    public NotificationEmailCheck(
        ILocalizedTextService textService,
        IOptionsMonitor<ContentSettings> contentSettings)
        : base(textService) =>
        _contentSettings = contentSettings;

    /// <inheritdoc />
    public override string ItemPath => Constants.Configuration.ConfigContentNotificationsEmail;

    /// <inheritdoc />
    public override ValueComparisonType ValueComparisonType => ValueComparisonType.ShouldNotEqual;

    /// <inheritdoc />
    public override IEnumerable<AcceptableConfiguration> Values => new List<AcceptableConfiguration>
    {
        new() { IsRecommended = false, Value = DefaultFromEmail }, new() { IsRecommended = false, Value = string.Empty },
    };

    /// <inheritdoc />
    public override string CurrentValue => _contentSettings.CurrentValue.Notifications.Email ?? string.Empty;

    /// <inheritdoc />
    public override string CheckSuccessMessage =>
        LocalizedTextService.Localize("healthcheck", "notificationEmailsCheckSuccessMessage", new[] { CurrentValue ?? "&lt;null&gt;" });

    /// <inheritdoc />
    public override string CheckErrorMessage => LocalizedTextService.Localize(
        "healthcheck",
        "notificationEmailsCheckErrorMessage",
        new[] { DefaultFromEmail });

    /// <inheritdoc />
    public override string ReadMoreLink =>
        Constants.HealthChecks.DocumentationLinks.Configuration.NotificationEmailCheck;
}
