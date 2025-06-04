using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.TemporaryFile;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Validates a temporary file upload.
/// </summary>
public class TemporaryFileUploadValidator : IValueValidator
{
    private readonly GetContentSettings _getContentSettings;
    private readonly ParseTemporaryFileKey _parseTemporaryFileKey;
    private readonly GetTemporaryFileModel _getTemporaryFileModel;
    private readonly ValidateFileType? _validateFileType;

    public delegate ContentSettings GetContentSettings();

    public delegate Guid? ParseTemporaryFileKey(object? editorValue);

    public delegate TemporaryFileModel? GetTemporaryFileModel(Guid temporaryFileKey);

    public delegate bool ValidateFileType(string extension, object? dataTypeConfiguration);

    /// <summary>
    /// Initializes a new instance of the <see cref="TemporaryFileUploadValidator"/> class.
    /// </summary>
    public TemporaryFileUploadValidator(
        GetContentSettings getContentSettings,
        ParseTemporaryFileKey parseTemporaryFileKey,
        GetTemporaryFileModel getTemporaryFileModel,
        ValidateFileType? validateFileType = null)
    {
        _getContentSettings = getContentSettings;
        _parseTemporaryFileKey = parseTemporaryFileKey;
        _getTemporaryFileModel = getTemporaryFileModel;
        _validateFileType = validateFileType;
    }

    /// <inheritdoc/>
    public IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration, PropertyValidationContext validationContext)
    {
        Guid? temporaryFileKey = _parseTemporaryFileKey(value);
        if (temporaryFileKey.HasValue == false)
        {
            yield break;
        }

        TemporaryFileModel? temporaryFile = _getTemporaryFileModel(temporaryFileKey.Value);
        if (temporaryFile == null)
        {
            yield return new ValidationResult(
                $"No temporary file was found for the key: {temporaryFileKey.Value}",
                ["value"]);
        }
        else
        {
            var extension = Path.GetExtension(temporaryFile.FileName).TrimStart('.');
            if (extension.IsNullOrWhiteSpace())
            {
                yield break;
            }

            ContentSettings contentSettings = _getContentSettings();
            if (contentSettings.IsFileAllowedForUpload(extension) is false || (_validateFileType != null && _validateFileType(extension, dataTypeConfiguration) == false))
            {
                yield return new ValidationResult(
                    $"The file type for file name \"{temporaryFile.FileName}\" is not valid for upload",
                    ["value"]);
            }
        }
    }
}
