using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.TemporaryFile;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Provides validation logic for files uploaded temporarily via property editors in Umbraco.
/// Ensures that uploaded files meet required criteria before being processed or stored permanently.
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
    /// <param name="getContentSettings">A delegate of type <see cref="GetContentSettings"/> used to retrieve content settings.</param>
    /// <param name="parseTemporaryFileKey">A delegate of type <see cref="ParseTemporaryFileKey"/> used to parse the temporary file key.</param>
    /// <param name="getTemporaryFileModel">A delegate of type <see cref="GetTemporaryFileModel"/> used to retrieve the temporary file model.</param>
    /// <param name="validateFileType">An optional delegate of type <see cref="ValidateFileType"/> used to validate the file type. If <c>null</c>, file type validation is not performed.</param>
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

    /// <summary>
    /// Validates a temporary file upload by checking if the file exists and whether its type is allowed based on the provided configuration.
    /// </summary>
    /// <param name="value">The value representing the temporary file key, typically a <see cref="Guid"/> or string convertible to a GUID.</param>
    /// <param name="valueType">The type of the value, if specified; may be used for additional validation logic.</param>
    /// <param name="dataTypeConfiguration">The configuration object for the data type, used to determine allowed file types.</param>
    /// <param name="validationContext">The context for property validation, providing additional information about the validation request.</param>
    /// <returns>
    /// An <see cref="IEnumerable{ValidationResult}"/> containing validation errors if the file does not exist or its type is not allowed; otherwise, an empty sequence.
    /// </returns>
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
                new[] { "value" });
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
                    new[] { "value" });
            }
        }
    }
}
