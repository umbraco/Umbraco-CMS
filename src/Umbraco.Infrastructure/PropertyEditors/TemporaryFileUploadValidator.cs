﻿using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.TemporaryFile;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

internal class TemporaryFileUploadValidator : IValueValidator
{
    private readonly GetContentSettings _getContentSettings;
    private readonly ParseTemporaryFileKey _parseTemporaryFileKey;
    private readonly GetTemporaryFileModel _getTemporaryFileModel;
    private readonly ValidateFileType? _validateFileType;

    internal delegate ContentSettings GetContentSettings();

    internal delegate Guid? ParseTemporaryFileKey(object? editorValue);

    internal delegate TemporaryFileModel? GetTemporaryFileModel(Guid temporaryFileKey);

    internal delegate bool ValidateFileType(string extension, object? dataTypeConfiguration);

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

    public IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration)
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
