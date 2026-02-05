// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Models.TemporaryFile;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.PropertyEditors;
using Umbraco.Cms.Infrastructure.PropertyEditors.Validators;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     The value editor for the file upload property editor.
/// </summary>
/// <remarks>
///     As this class is loaded into <see cref="ValueEditorCache"/> which can be cleared, it needs
///     to be disposable in order to properly clean up resources such as
///     the settings change subscription and avoid a memory leak.
/// </remarks>
public class FileUploadPropertyValueEditor : DataValueEditor, IDisposable
{
    private bool _disposed;

    private readonly MediaFileManager _mediaFileManager;
    private readonly ITemporaryFileService _temporaryFileService;
    private readonly IScopeProvider _scopeProvider;
    private readonly IFileStreamSecurityValidator _fileStreamSecurityValidator;
    private readonly FileUploadValueParser _valueParser;

    private ContentSettings _contentSettings;
    private readonly IDisposable? _contentSettingsChangeSubscription;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileUploadPropertyValueEditor"/> class.
    /// </summary>
    public FileUploadPropertyValueEditor(
        DataEditorAttribute attribute,
        MediaFileManager mediaFileManager,
        IShortStringHelper shortStringHelper,
        IOptionsMonitor<ContentSettings> contentSettings,
        IJsonSerializer jsonSerializer,
        IIOHelper ioHelper,
        ITemporaryFileService temporaryFileService,
        IScopeProvider scopeProvider,
        IFileStreamSecurityValidator fileStreamSecurityValidator)
        : base(shortStringHelper, jsonSerializer, ioHelper, attribute)
    {
        _mediaFileManager = mediaFileManager;
        _temporaryFileService = temporaryFileService;
        _scopeProvider = scopeProvider;
        _fileStreamSecurityValidator = fileStreamSecurityValidator;
        _valueParser = new FileUploadValueParser(jsonSerializer);

        _contentSettings = contentSettings.CurrentValue;
        _contentSettingsChangeSubscription = contentSettings.OnChange(x => _contentSettings = x);

        Validators.Add(new TemporaryFileUploadValidator(
            () => _contentSettings,
            TryParseTemporaryFileKey,
            TryGetTemporaryFile,
            IsAllowedInDataTypeConfiguration));
    }

    /// <inheritdoc/>
    public override IValueRequiredValidator RequiredValidator => new FileUploadValueRequiredValidator();

    /// <inheritdoc/>
    public override object? ToEditor(IProperty property, string? culture = null, string? segment = null)
    {
        // the stored property value (if any) is the path to the file; convert it to the client model (FileUploadValue)
        var propertyValue = property.GetValue(culture, segment);
        return propertyValue is string stringValue
            ? new FileUploadValue
            {
                Src = stringValue,
            }
            : null;
    }

    /// <inheritdoc/>
    /// <summary>
    ///     Converts the client model (FileUploadValue) into the value can be stored in the database (the file path).
    /// </summary>
    /// <param name="editorValue">The value received from the editor.</param>
    /// <param name="currentValue">The current value of the property</param>
    /// <returns>The converted value.</returns>
    /// <remarks>
    ///     <para>The <paramref name="currentValue" /> is used to re-use the folder, if possible.</para>
    ///     <para>
    ///         The <paramref name="editorValue" /> is value passed in from the editor. If the value is empty, we
    ///         must delete the currently selected file (<paramref name="currentValue" />).
    ///     </para>
    /// </remarks>
    public override object? FromEditor(ContentPropertyData editorValue, object? currentValue)
    {
        FileUploadValue? editorModelValue = _valueParser.Parse(editorValue.Value);

        // No change or created from blueprint.
        if (editorModelValue?.TemporaryFileId.HasValue is not true && string.IsNullOrEmpty(editorModelValue?.Src) is false)
        {
            return editorModelValue.Src;
        }

        // the current editor value (if any) is the path to the file
        var currentPath = currentValue is string currentStringValue
                          && currentStringValue.IsNullOrWhiteSpace() is false
            ? _mediaFileManager.FileSystem.GetRelativePath(currentStringValue)
            : null;

        // resetting the current value?
        if (string.IsNullOrEmpty(editorModelValue?.Src) && currentPath.IsNullOrWhiteSpace() is false)
        {
            // delete the current file and clear the value of this property
            _mediaFileManager.FileSystem.DeleteFile(currentPath);
            return null;
        }

        Guid? temporaryFileKey = editorModelValue?.TemporaryFileId;
        TemporaryFileModel? file = temporaryFileKey is null ? null : TryGetTemporaryFile(temporaryFileKey.Value);
        if (file is null)
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

        return filepath is null ? null : _mediaFileManager.FileSystem.GetUrl(filepath);
    }

    private Guid? TryParseTemporaryFileKey(object? editorValue)
        => _valueParser.Parse(editorValue)?.TemporaryFileId;

    private TemporaryFileModel? TryGetTemporaryFile(Guid temporaryFileKey)
        => _temporaryFileService.GetAsync(temporaryFileKey).GetAwaiter().GetResult();

    private static bool IsAllowedInDataTypeConfiguration(string extension, object? dataTypeConfiguration)
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
        string filepath = GetMediaPath(file, dataTypeConfiguration, contentKey, propertyTypeKey);

        using (Stream filestream = file.OpenReadStream())
        {
            if (_fileStreamSecurityValidator.IsConsideredSafe(filestream) == false)
            {
                return null;
            }

            // TODO: Here it would make sense to do the auto-fill properties stuff but the API doesn't allow us to do that right
            // since we'd need to be able to return values for other properties from these methods
            _mediaFileManager.FileSystem.AddFile(filepath, filestream, overrideIfExists: true); // must overwrite!
        }

        return filepath;
    }

    /// <summary>
    /// Provides media path.
    /// </summary>
    /// <returns>File system relative path</returns>
    protected virtual string GetMediaPath(TemporaryFileModel file, object? dataTypeConfiguration, Guid contentKey, Guid propertyTypeKey)
    {
        // in case we are using the old path scheme, try to re-use numbers (bah...)
        return _mediaFileManager.GetMediaPath(file.FileName, contentKey, propertyTypeKey);
    }

    /// <summary>
    /// Disposes the object.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the object.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _contentSettingsChangeSubscription?.Dispose();
        }

        _disposed = true;
    }
}
