// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Models.TemporaryFile;
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
    private readonly ITemporaryFileService _temporaryFileService;
    private readonly IScopeProvider _scopeProvider;
    private ContentSettings _contentSettings;

    public FileUploadPropertyValueEditor(
        DataEditorAttribute attribute,
        MediaFileManager mediaFileManager,
        ILocalizedTextService localizedTextService,
        IShortStringHelper shortStringHelper,
        IOptionsMonitor<ContentSettings> contentSettings,
        IJsonSerializer jsonSerializer,
        IIOHelper ioHelper,
        ITemporaryFileService temporaryFileService,
        IScopeProvider scopeProvider)
        : base(localizedTextService, shortStringHelper, jsonSerializer, ioHelper, attribute)
    {
        _mediaFileManager = mediaFileManager ?? throw new ArgumentNullException(nameof(mediaFileManager));
        _temporaryFileService = temporaryFileService;
        _scopeProvider = scopeProvider;
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
        var currentStringValue = currentValue as string;
        currentStringValue = currentStringValue.NullOrWhiteSpaceAsNull();

        var editorStringValue = editorValue.Value as string;
        editorStringValue = editorStringValue.NullOrWhiteSpaceAsNull();

        // no change?
        if (editorStringValue == currentStringValue)
        {
            return currentValue;
        }

        var currentPath = currentStringValue;
        if (currentPath.IsNullOrWhiteSpace() == false)
        {
            currentPath = _mediaFileManager.FileSystem.GetRelativePath(currentPath);
        }

        // resetting the current value?
        if (editorStringValue is null && currentPath.IsNullOrWhiteSpace() is false)
        {
            // delete the current file and clear the value of this property
            _mediaFileManager.FileSystem.DeleteFile(currentPath);
            return null;
        }

        // uploading a file?
        if (Guid.TryParse(editorStringValue, out Guid temporaryFileKey) == false)
        {
            return null;
        }

        TemporaryFileModel? file = TryGetTemporaryFile(temporaryFileKey);
        if (file == null)
        {
            // at this point the temporary file *should* have been validated by TemporaryFileUploadValidator, so we
            // should never end up here. In case we do, let's attempt to at least be non-destructive by returning
            // the current value
            return currentValue;
        }

        // schedule temporary file for deletion
        using IScope scope = _scopeProvider.CreateScope();
        _temporaryFileService.EnlistDeleteIfScopeCompletes(temporaryFileKey, _scopeProvider);

        // ensure we have the required guids
        Guid cuid = editorValue.ContentKey;
        if (cuid == Guid.Empty)
        {
            throw new Exception("Invalid content key.");
        }

        Guid puid = editorValue.PropertyTypeKey;
        if (puid == Guid.Empty)
        {
            throw new Exception("Invalid property type key.");
        }

        // process the file
        var filepath = ProcessFile(file, editorValue.DataTypeConfiguration, cuid, puid);

        // remove current file if replaced
        if (currentPath != filepath && currentPath.IsNullOrWhiteSpace() is false)
        {
            _mediaFileManager.FileSystem.DeleteFile(currentPath);
        }

        scope.Complete();

        return filepath == null ? null : _mediaFileManager.FileSystem.GetUrl(filepath);
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
            return fileUploadConfiguration.FileExtensions.IsCollectionEmpty() ||
                   fileUploadConfiguration.FileExtensions.Any(x => x.Value?.InvariantEquals(extension) ?? false);
        }

        return false;
    }

    private string? ProcessFile(TemporaryFileModel file, object? dataTypeConfiguration, Guid cuid, Guid puid)
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
        var filepath = _mediaFileManager.GetMediaPath(file.FileName, cuid, puid); // fs-relative path

        using (Stream filestream = file.OpenReadStream())
        {
            // TODO: Here it would make sense to do the auto-fill properties stuff but the API doesn't allow us to do that right
            // since we'd need to be able to return values for other properties from these methods
            _mediaFileManager.FileSystem.AddFile(filepath, filestream, true); // must overwrite!
        }

        return filepath;
    }
}
