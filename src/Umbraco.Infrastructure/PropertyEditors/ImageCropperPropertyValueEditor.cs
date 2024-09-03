// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
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
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     The value editor for the image cropper property editor.
/// </summary>
internal class ImageCropperPropertyValueEditor : DataValueEditor // TODO: core vs web?
{
    private readonly IDataTypeConfigurationCache _dataTypeConfigurationCache;
    private readonly IFileStreamSecurityValidator _fileStreamSecurityValidator;
    private readonly ILogger<ImageCropperPropertyValueEditor> _logger;
    private readonly MediaFileManager _mediaFileManager;
    private readonly IJsonSerializer _jsonSerializer;
    private ContentSettings _contentSettings;
    private readonly ITemporaryFileService _temporaryFileService;
    private readonly IScopeProvider _scopeProvider;

    public ImageCropperPropertyValueEditor(
        DataEditorAttribute attribute,
        ILogger<ImageCropperPropertyValueEditor> logger,
        MediaFileManager mediaFileSystem,
        ILocalizedTextService localizedTextService,
        IShortStringHelper shortStringHelper,
        IOptionsMonitor<ContentSettings> contentSettings,
        IJsonSerializer jsonSerializer,
        IIOHelper ioHelper,
        ITemporaryFileService temporaryFileService,
        IScopeProvider scopeProvider,
        IFileStreamSecurityValidator fileStreamSecurityValidator,
        IDataTypeConfigurationCache dataTypeConfigurationCache)
        : base(localizedTextService, shortStringHelper, jsonSerializer, ioHelper, attribute)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mediaFileManager = mediaFileSystem ?? throw new ArgumentNullException(nameof(mediaFileSystem));
        _jsonSerializer = jsonSerializer;
        _contentSettings = contentSettings.CurrentValue;
        _temporaryFileService = temporaryFileService;
        _scopeProvider = scopeProvider;
        _fileStreamSecurityValidator = fileStreamSecurityValidator;
        _dataTypeConfigurationCache = dataTypeConfigurationCache;
        contentSettings.OnChange(x => _contentSettings = x);

        Validators.Add(new TemporaryFileUploadValidator(() => _contentSettings, TryParseTemporaryFileKey, TryGetTemporaryFile));
    }

    /// <summary>
    ///     This is called to merge in the prevalue crops with the value that is saved - similar to the property value
    ///     converter for the front-end
    /// </summary>
    public override object? ToEditor(IProperty property, string? culture = null, string? segment = null)
    {
        var val = property.GetValue(culture, segment);
        if (val == null)
        {
            return null;
        }

        ImageCropperValue? value;
        try
        {
            value = _jsonSerializer.Deserialize<ImageCropperValue>(val.ToString()!);
        }
        catch
        {
            value = new ImageCropperValue { Src = val.ToString() };
        }

        ImageCropperConfiguration? configuration = _dataTypeConfigurationCache.GetConfigurationAs<ImageCropperConfiguration>(property.PropertyType.DataTypeKey);
        if (configuration is not null)
        {
            value?.ApplyConfiguration(configuration);
        }

        return value;
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
    ///         editorValue.Value is used to figure out editorFile and, if it has been cleared, remove the old file.
    ///         If editorValue.Value deserializes as <see cref="ImageCropperValue"/> and the <see cref="ImageCropperValue.Src"/>
    ///         value is a GUID, it is assumed to contain a temporary file key, and we will attempt to replace the currently
    ///         selected file with the corresponding temporary file.
    ///     </para>
    /// </remarks>
    public override object? FromEditor(ContentPropertyData editorValue, object? currentValue)
    {
        // Get the current path
        var currentPath = string.Empty;
        try
        {
            if (currentValue is string currentStringValue)
            {
                ImageCropperValue? currentImageCropperValue = _jsonSerializer.Deserialize<ImageCropperValue>(currentStringValue);
                currentPath = currentImageCropperValue?.Src;
            }
        }
        catch (Exception ex)
        {
            // For some reason the value is invalid so continue as if there was no value there
            _logger.LogWarning(ex, "Could not parse current db value to an ImageCropperValue object.");
        }

        if (string.IsNullOrWhiteSpace(currentPath) == false)
        {
            currentPath = _mediaFileManager.FileSystem.GetRelativePath(currentPath);
        }

        ImageCropperValue? editorImageCropperValue = ParseImageCropperValue(editorValue.Value);

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

        using IScope scope = _scopeProvider.CreateScope();

        TemporaryFileModel? file = null;
        Guid? temporaryFileKey = editorImageCropperValue?.TemporaryFileId;
        if (temporaryFileKey.HasValue)
        {
            file = TryGetTemporaryFile(temporaryFileKey.Value);
            if (file is not null)
            {
                _temporaryFileService.EnlistDeleteIfScopeCompletes(temporaryFileKey.Value, _scopeProvider);
            }
        }

        if (file == null) // not uploading a file
        {
            // if editorFile is empty then either there was nothing to begin with,
            // or it has been cleared and we need to remove the file - else the
            // value is unchanged.
            if (string.IsNullOrWhiteSpace(editorImageCropperValue?.Src) && string.IsNullOrWhiteSpace(currentPath) is false)
            {
                _mediaFileManager.FileSystem.DeleteFile(currentPath);
                return null; // clear
            }

            if (editorImageCropperValue is not null && temporaryFileKey.HasValue)
            {
                // a plausible tempFile value was supplied, but could not be converted to an actual file => clear the src
                editorImageCropperValue.Src = null;
            }
            return _jsonSerializer.Serialize(editorImageCropperValue); // unchanged
        }

        // process the file
        var filepath = editorImageCropperValue == null ? null : ProcessFile(file, contentKey, propertyTypeKey);

        // remove current file if replaced
        if (currentPath != filepath && string.IsNullOrWhiteSpace(currentPath) == false)
        {
            _mediaFileManager.FileSystem.DeleteFile(currentPath);
        }

        scope.Complete();

        // update json and return
        if (editorImageCropperValue == null)
        {
            return null;
        }

        editorImageCropperValue.Src = filepath is null ? string.Empty : _mediaFileManager.FileSystem.GetUrl(filepath);
        editorImageCropperValue.TemporaryFileId = null;
        return _jsonSerializer.Serialize(editorImageCropperValue);
    }

    public override string ConvertDbToString(IPropertyType propertyType, object? value)
    {
        if (value == null || string.IsNullOrEmpty(value.ToString()))
        {
            return string.Empty;
        }

        // if we don't have a json structure, we will get it from the property type
        var val = value.ToString();
        if (val?.DetectIsJson() ?? false)
        {
            return val;
        }

        // more magic here ;-(
        ImageCropperConfiguration? configuration = _dataTypeConfigurationCache.GetConfigurationAs<ImageCropperConfiguration>(propertyType.DataTypeKey);
        ImageCropperConfiguration.Crop[] crops = configuration?.Crops ?? Array.Empty<ImageCropperConfiguration.Crop>();

        return _jsonSerializer.Serialize(new { src = val, crops });
    }

    private ImageCropperValue? ParseImageCropperValue(object? editorValue)
    {
        if (editorValue is null)
        {
            return null;
        }

        return _jsonSerializer.TryDeserialize(editorValue, out ImageCropperValue? imageCropperValue)
            ? imageCropperValue
            : throw new ArgumentException($"Could not parse editor value to a {nameof(ImageCropperValue)} object.");
    }

    private Guid? TryParseTemporaryFileKey(object? editorValue)
        => ParseImageCropperValue(editorValue)?.TemporaryFileId;

    private TemporaryFileModel? TryGetTemporaryFile(Guid temporaryFileKey)
        => _temporaryFileService.GetAsync(temporaryFileKey).GetAwaiter().GetResult();

    private string? ProcessFile(TemporaryFileModel file, Guid contentKey, Guid propertyTypeKey)
    {
        // process the file
        // no file, invalid file, reject change
        // this check is somewhat redundant as the file validity has already been checked by TemporaryFileUploadValidator,
        // but we'll retain it here as a last measure in case someone accidentally breaks the validator
        var extension = Path.GetExtension(file.FileName).TrimStart('.');
        if (_contentSettings.IsFileAllowedForUpload(extension) is false)
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
