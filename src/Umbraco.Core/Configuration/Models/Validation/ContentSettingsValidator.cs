// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Core.Configuration.Models.Validation;

/// <summary>
///     Validator for configuration representated as <see cref="ContentSettings" />.
/// </summary>
public class ContentSettingsValidator : ConfigurationValidatorBase, IValidateOptions<ContentSettings>
{
    private readonly ILogger<ContentSettingsValidator> _logger;

    private string? _lastCheckedPreviewBadge;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentSettingsValidator" /> class.
    /// </summary>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 19.")]
    public ContentSettingsValidator()
        : this(StaticServiceProvider.Instance.GetRequiredService<ILogger<ContentSettingsValidator>>())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentSettingsValidator" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public ContentSettingsValidator(ILogger<ContentSettingsValidator> logger)
        => _logger = logger;

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, ContentSettings options)
    {
        if (!ValidateError404Collection(options.Error404Collection, out var message))
        {
            return ValidateOptionsResult.Fail(message);
        }

        if (!ValidateAutoFillImageProperties(options.Imaging.AutoFillImageProperties, out message))
        {
            return ValidateOptionsResult.Fail(message);
        }

        WarnOnPreviewBadgeWithoutNoncePlaceholder(options);

        return ValidateOptionsResult.Success;
    }

    private bool ValidateError404Collection(IEnumerable<ContentErrorPage> values, out string message) =>
        ValidateCollection(
            $"{Constants.Configuration.ConfigContent}:{nameof(ContentSettings.Error404Collection)}",
            values,
            "Culture and one and only one of ContentId, ContentKey and ContentXPath must be specified for each entry",
            out message);

    private bool ValidateAutoFillImageProperties(IEnumerable<ImagingAutoFillUploadField> values, out string message) =>
        ValidateCollection(
            $"{Constants.Configuration.ConfigContent}:{nameof(ContentSettings.Imaging)}:{nameof(ContentSettings.Imaging.AutoFillImageProperties)}",
            values,
            "Alias, WidthFieldAlias, HeightFieldAlias, LengthFieldAlias and ExtensionFieldAlias must be specified for each entry",
            out message);

    private void WarnOnPreviewBadgeWithoutNoncePlaceholder(ContentSettings options)
    {
        // Validation runs once per options instance rather than once per process, and the scoped
        // IOptionsSnapshot rebuilds per request, so only warn when the configured value changes.
        if (Interlocked.Exchange(ref _lastCheckedPreviewBadge, options.PreviewBadge) == options.PreviewBadge)
        {
            return;
        }

        // The default mark-up carries the placeholder, so a value without it has been customised; an empty
        // value disables the badge altogether. See https://github.com/umbraco/Umbraco-CMS/issues/23530.
        if (string.IsNullOrWhiteSpace(options.PreviewBadge)
            || options.PreviewBadge.Contains(ContentSettings.PreviewBadgeNoncePlaceholder, StringComparison.Ordinal))
        {
            return;
        }

        _logger.LogWarning(
            "Configuration entry {ConfigPath} has been customised without the {Placeholder} placeholder, so the preview badge script tag is rendered without a CSP nonce and will be blocked if the site's Content-Security-Policy relies on one. Add the placeholder to the opening script tag to resolve this.",
            $"{Constants.Configuration.ConfigContent}:{nameof(ContentSettings.PreviewBadge)}",
            ContentSettings.PreviewBadgeNoncePlaceholder);
    }
}
