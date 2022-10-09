// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;

namespace Umbraco.Cms.Core.Configuration.Models.Validation;

/// <summary>
///     Validator for configuration representated as <see cref="RequestHandlerSettings" />.
/// </summary>
public class RequestHandlerSettingsValidator : ConfigurationValidatorBase, IValidateOptions<RequestHandlerSettings>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string name, RequestHandlerSettings options)
    {
        if (!ValidateConvertUrlsToAscii(options.ConvertUrlsToAscii, out var message))
        {
            return ValidateOptionsResult.Fail(message);
        }

        return ValidateOptionsResult.Success;
    }

    private bool ValidateConvertUrlsToAscii(string value, out string message)
    {
        var validValues = new[] { "try", "true", "false" };
        return ValidateStringIsOneOfValidValues(
            $"{Constants.Configuration.ConfigRequestHandler}:{nameof(RequestHandlerSettings.ConvertUrlsToAscii)}", value, validValues, out message);
    }
}
