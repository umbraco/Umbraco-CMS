// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Models.TemporaryFile;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     The value editor for the file upload property editor.
/// </summary>
internal class FileUploadPropertyValueEditor : DataValueEditor
{
    private readonly MediaFileManager _mediaFileManager;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ITemporaryFileService _temporaryFileService;
    private readonly IScopeProvider _scopeProvider;
    private readonly IFileStreamSecurityValidator _fileStreamSecurityValidator;
    private readonly ILogger<FileUploadPropertyValueEditor> _logger;
    private ContentSettings _contentSettings;

    [Obsolete("Use the non-obsolete constructor, scheduled for removal in V15")]
    public FileUploadPropertyValueEditor(
        DataEditorAttribute attribute,
        MediaFileManager mediaFileManager,
        ILocalizedTextService localizedTextService,
        IShortStringHelper shortStringHelper,
        IOptionsMonitor<ContentSettings> contentSettings,
        IJsonSerializer jsonSerializer,
        IIOHelper ioHelper,
        ITemporaryFileService temporaryFileService,
        IScopeProvider scopeProvider,
        IFileStreamSecurityValidator fileStreamSecurityValidator)
    :this(attribute, mediaFileManager, localizedTextService, shortStringHelper, contentSettings, jsonSerializer, ioHelper, temporaryFileService, scopeProvider, fileStreamSecurityValidator, StaticServiceProvider.Instance.GetRequiredService<ILogger<FileUploadPropertyValueEditor>>())
    {

    }

    public FileUploadPropertyValueEditor(
        DataEditorAttribute attribute,
        MediaFileManager mediaFileManager,
        ILocalizedTextService localizedTextService,
        IShortStringHelper shortStringHelper,
        IOptionsMonitor<ContentSettings> contentSettings,
        IJsonSerializer jsonSerializer,
        IIOHelper ioHelper,
        ITemporaryFileService temporaryFileService,
        IScopeProvider scopeProvider,
        IFileStreamSecurityValidator fileStreamSecurityValidator, ILogger<FileUploadPropertyValueEditor> logger)
        : base(localizedTextService, shortStringHelper, jsonSerializer, ioHelper, attribute)
    {
        _mediaFileManager = mediaFileManager ?? throw new ArgumentNullException(nameof(mediaFileManager));
        _jsonSerializer = jsonSerializer;
        _temporaryFileService = temporaryFileService;
        _scopeProvider = scopeProvider;
        _fileStreamSecurityValidator = fileStreamSecurityValidator;
        _logger = logger;
        _contentSettings = contentSettings.CurrentValue ?? throw new ArgumentNullException(nameof(contentSettings));
        contentSettings.OnChange(x => _contentSettings = x);

        Validators.Add(new TemporaryFileUploadValidator(
            () => _contentSettings,
            TryParseTemporaryFileKey,
            TryGetTemporaryFile,
            IsAllowedInDataTypeConfiguration));
    }

    /// <summary>
    ///     Converts the value received from the editor into the value can be stored in the database.
    /// </summary>
    /// <param name="editorValue">The value received from the editor.</param>
    /// <param name="currentValue">The current value of the property</param>
    /// <returns>The converted value.</returns>
    /// <remarks>
    ///     <para>The <paramref name="currentValue" /> is used to re-use the folder, if possible.</para>
    ///     <para>
    ///         The <paramref name="editorValue" /> is value passed in from the editor. If the value is empty, we
    ///         must delete the currently selected file (<paramref name="currentValue" />). If the value is not empty,
    ///         it is assumed to contain a temporary file key, and we will attempt to replace the currently selected
    ///         file with the corresponding temporary file.
    ///     </para>
    /// </remarks>
    public override object? FromEditor(ContentPropertyData editorValue, object? currentValue)
    {

        FileUploadValue? currentModelValue = null;
        var currentPath = string.Empty;
        try
        {
            if (currentValue is string currentStringValue)
            {
                currentModelValue = TryParseFileUploadValue(currentStringValue);
            }
        }
        catch (Exception ex)
        {
            // For some reason the value is invalid so continue as if there was no value there
            _logger.LogWarning(ex, "Could not parse current db value to an ImageCropperValue object.");
        }


        // no change?
        FileUploadValue? editorModelValue = TryParseFileUploadValue(editorValue.Value as string);

        if (Equals(editorModelValue, currentModelValue))
        {
            return currentValue;
        }

        if (currentPath.IsNullOrWhiteSpace() == false)
        {
            currentPath = _mediaFileManager.FileSystem.GetRelativePath(currentPath);
        }

        // resetting the current value?
        if (editorModelValue is null && currentPath.IsNullOrWhiteSpace() is false)
        {
            // delete the current file and clear the value of this property
            _mediaFileManager.FileSystem.DeleteFile(currentPath);
            return null;
        }

        var temporaryFileKey = editorModelValue?.TemporaryFileId;
        TemporaryFileModel? file = temporaryFileKey is null ? null : TryGetTemporaryFile(temporaryFileKey!.Value);
        if (file == null)
        {
            // at this point the temporary file *should* have been validated by TemporaryFileUploadValidator, so we
            // should never end up here. In case we do, let's attempt to at least be non-destructive by returning
            // the current value
            return currentValue;
        }

        // schedule temporary file for deletion
        using IScope scope = _scopeProvider.CreateScope();
        _temporaryFileService.EnlistDeleteIfScopeCompletes(temporaryFileKey!.Value, _scopeProvider);

        // ensure we have the required guids
        Guid contentKey = editorValue.ContentKey;
        if (contentKey == Guid.Empty)
        {
            throw new Exception("Invalid content key.");
        }

        Guid propertyTypeKey = editorValue.PropertyTypeKey;
        if (propertyTypeKey == Guid.Empty)
        {
            throw new Exception("Invalid property type key.");
        }

        // process the file
        var filepath = ProcessFile(file, editorValue.DataTypeConfiguration, contentKey, propertyTypeKey);

        // remove current file if replaced
        if (currentPath != filepath && currentPath.IsNullOrWhiteSpace() is false)
        {
            _mediaFileManager.FileSystem.DeleteFile(currentPath);
        }

        scope.Complete();

        return
            _jsonSerializer.Serialize(new FileUploadValue()
            {
                TemporaryFileId = null,
                Src = filepath == null ? null : _mediaFileManager.FileSystem.GetUrl(filepath)
            });

    }

    private FileUploadValue? TryParseFileUploadValue(string? value)
    {
        try
        {
            if (value is null)
            {
                return null;
            }

            if(_jsonSerializer.TryDeserialize(value, out FileUploadValue? modelValue))
            {
                return modelValue;
            }

            if(Guid.TryParse(value, out Guid temporaryFileId))
            {
                return new FileUploadValue()
                {
                    TemporaryFileId = temporaryFileId,
                    Src = null,
                };
            }

            return new FileUploadValue()
            {
                TemporaryFileId = null,
                Src = value,
            };
        }
        catch (Exception ex)
        {
            // For some reason the value is invalid - log error and continue as if no value was saved
            _logger.LogWarning(ex, "Could not parse editor value to an FileUploadValue object.");
        }

        return null;
    }

    private Guid? TryParseTemporaryFileKey(object? editorValue)
        => editorValue is string stringValue && Guid.TryParse(stringValue, out Guid temporaryFileKey)
            ? temporaryFileKey
            : null;

    private TemporaryFileModel? TryGetTemporaryFile(Guid temporaryFileKey)
        => _temporaryFileService.GetAsync(temporaryFileKey).GetAwaiter().GetResult();

    private bool IsAllowedInDataTypeConfiguration(string extension, object? dataTypeConfiguration)
    {
        if (dataTypeConfiguration is FileUploadConfiguration fileUploadConfiguration)
        {
            // If FileExtensions is empty and no allowed extensions have been specified, we allow everything.
            // If there are any extensions specified, we need to check that the uploaded extension is one of them.
            return fileUploadConfiguration.FileExtensions.Any() is false ||
                   fileUploadConfiguration.FileExtensions.Contains(extension);
        }

        return false;
    }

    private string? ProcessFile(TemporaryFileModel file, object? dataTypeConfiguration, Guid contentKey, Guid propertyTypeKey)
    {
        // process the file
        // no file, invalid file, reject change
        // this check is somewhat redundant as the file validity has already been checked by TemporaryFileUploadValidator,
        // but we'll retain it here as a last measure in case someone accidentally breaks the validator
        var extension = Path.GetExtension(file.FileName).TrimStart('.');
        if (_contentSettings.IsFileAllowedForUpload(extension) is false ||
            IsAllowedInDataTypeConfiguration(extension, dataTypeConfiguration) is false)
        {
            return null;
        }

        // get the filepath
        // in case we are using the old path scheme, try to re-use numbers (bah...)
        var filepath = _mediaFileManager.GetMediaPath(file.FileName, contentKey, propertyTypeKey); // fs-relative path

        using (Stream filestream = file.OpenReadStream())
        {
            if (_fileStreamSecurityValidator.IsConsideredSafe(filestream) == false)
            {
                return null;
            }

            // TODO: Here it would make sense to do the auto-fill properties stuff but the API doesn't allow us to do that right
            // since we'd need to be able to return values for other properties from these methods
            _mediaFileManager.FileSystem.AddFile(filepath, filestream, true); // must overwrite!
        }

        return filepath;
    }
}
