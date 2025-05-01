// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Logging;

namespace Umbraco.Cms.Core.Configuration.Models.Validation;

/// <summary>
///     Validator for configuration representated as <see cref="LoggingSettings" />.
/// </summary>
public class LoggingSettingsValidator : ConfigurationValidatorBase, IValidateOptions<LoggingSettings>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, LoggingSettings options)
    {
        if (!ValidateFileNameFormatArgument(options.FileNameFormat, options.FileNameFormatArguments, out var message))
        {
            return ValidateOptionsResult.Fail(message);
        }


        return ValidateOptionsResult.Success;
    }

    private bool ValidateFileNameFormatArgument(string fileNameFormat, string fileNameFormatArguments, out string message)
    {
        var fileNameFormatArgumentsAsArray = fileNameFormatArguments
            .Split([','], StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .ToArray();
        if (fileNameFormatArgumentsAsArray.Any(x => LoggingConfiguration.SupportedFileNameFormatArguments.Contains(x) is false))
        {
            message = $"The file name arguments '{string.Join(",", fileNameFormatArgumentsAsArray)}' contain one or more values that aren't in the supported list of values '{string.Join(",", LoggingConfiguration.SupportedFileNameFormatArguments)}'.";
            return false;
        }

        try
        {
            _ = string.Format(fileNameFormat, fileNameFormatArgumentsAsArray);
        }
        catch (FormatException)
        {
            message = $"The provided file name format '{fileNameFormat}' could not be used with the provided arguments '{string.Join(",", fileNameFormatArgumentsAsArray)}'.";
            return false;
        }

        message = string.Empty;
        return true;
    }
}
