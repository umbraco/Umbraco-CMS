// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;

namespace Umbraco.Cms.Core.Configuration.Models.Validation;

/// <summary>
///     Validator for configuration representated as <see cref="ContentSettings" />.
/// </summary>
public class ContentSettingsValidator : ConfigurationValidatorBase, IValidateOptions<ContentSettings>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string name, ContentSettings options)
    {
        if (!ValidateError404Collection(options.Error404Collection, out var message))
        {
            return ValidateOptionsResult.Fail(message);
        }

        if (!ValidateAutoFillImageProperties(options.Imaging.AutoFillImageProperties, out message))
        {
            return ValidateOptionsResult.Fail(message);
        }

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
}
