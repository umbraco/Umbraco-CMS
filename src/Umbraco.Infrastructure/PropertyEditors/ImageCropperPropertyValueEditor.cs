// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;
using File = System.IO.File;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     The value editor for the image cropper property editor.
/// </summary>
internal class ImageCropperPropertyValueEditor : DataValueEditor // TODO: core vs web?
{
    private readonly IDataTypeService _dataTypeService;
    private readonly ILogger<ImageCropperPropertyValueEditor> _logger;
    private readonly MediaFileManager _mediaFileManager;
    private ContentSettings _contentSettings;

    public ImageCropperPropertyValueEditor(
        DataEditorAttribute attribute,
        ILogger<ImageCropperPropertyValueEditor> logger,
        MediaFileManager mediaFileSystem,
        ILocalizedTextService localizedTextService,
        IShortStringHelper shortStringHelper,
        IOptionsMonitor<ContentSettings> contentSettings,
        IJsonSerializer jsonSerializer,
        IIOHelper ioHelper,
        IDataTypeService dataTypeService)
        : base(localizedTextService, shortStringHelper, jsonSerializer, ioHelper, attribute)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mediaFileManager = mediaFileSystem ?? throw new ArgumentNullException(nameof(mediaFileSystem));
        _contentSettings = contentSettings.CurrentValue;
        _dataTypeService = dataTypeService;
        contentSettings.OnChange(x => _contentSettings = x);
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
            value = JsonConvert.DeserializeObject<ImageCropperValue>(val.ToString()!);
        }
        catch
        {
            value = new ImageCropperValue { Src = val.ToString() };
        }

        IDataType? dataType = _dataTypeService.GetDataType(property.PropertyType.DataTypeId);
        if (dataType?.Configuration != null)
        {
            value?.ApplyConfiguration(dataType.ConfigurationAs<ImageCropperConfiguration>());
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
    ///         editorValue.Value is used to figure out editorFile and, if it has been cleared, remove the old file - but
    ///         it is editorValue.AdditionalData["files"] that is used to determine the actual file that has been uploaded.
    ///     </para>
    /// </remarks>
    public override object? FromEditor(ContentPropertyData editorValue, object? currentValue)
    {
        // Get the current path
        var currentPath = string.Empty;
        try
        {
            var svalue = currentValue as string;
            JObject? currentJson = string.IsNullOrWhiteSpace(svalue) ? null : JObject.Parse(svalue);
            if (currentJson != null && currentJson.TryGetValue("src", out JToken? src))
            {
                currentPath = src.Value<string>();
            }
        }
        catch (Exception ex)
        {
            // For some reason the value is invalid so continue as if there was no value there
            _logger.LogWarning(ex, "Could not parse current db value to a JObject.");
        }

        if (string.IsNullOrWhiteSpace(currentPath) == false)
        {
            currentPath = _mediaFileManager.FileSystem.GetRelativePath(currentPath);
        }

        // Get the new JSON and file path
        var editorFile = string.Empty;
        var editorJson = (JObject?)editorValue.Value;
        if (editorJson is not null)
        {
            // Populate current file
            if (editorJson["src"] != null)
            {
                editorFile = editorJson["src"]?.Value<string>();
            }

            // Clean up redundant/default data
            ImageCropperValue.Prune(editorJson);
        }
        else
        {
            editorJson = null;
        }

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

        // editorFile is empty whenever a new file is being uploaded
        // or when the file is cleared (in which case editorJson is null)
        // else editorFile contains the unchanged value
        ContentPropertyFile[]? uploads = editorValue.Files;
        if (uploads == null)
        {
            throw new Exception("Invalid files.");
        }

        ContentPropertyFile? file = uploads.Length > 0 ? uploads[0] : null;

        if (file == null) // not uploading a file
        {
            // if editorFile is empty then either there was nothing to begin with,
            // or it has been cleared and we need to remove the file - else the
            // value is unchanged.
            if (string.IsNullOrWhiteSpace(editorFile) && string.IsNullOrWhiteSpace(currentPath) == false)
            {
                _mediaFileManager.FileSystem.DeleteFile(currentPath);
                return null; // clear
            }

            return editorJson?.ToString(Formatting.None); // unchanged
        }

        // process the file
        var filepath = editorJson == null ? null : ProcessFile(file, cuid, puid);

        // remove all temp files
        foreach (ContentPropertyFile f in uploads)
        {
            File.Delete(f.TempFilePath);
        }

        // remove current file if replaced
        if (currentPath != filepath && string.IsNullOrWhiteSpace(currentPath) == false)
        {
            _mediaFileManager.FileSystem.DeleteFile(currentPath);
        }

        // update json and return
        if (editorJson == null)
        {
            return null;
        }

        editorJson["src"] = filepath == null ? string.Empty : _mediaFileManager.FileSystem.GetUrl(filepath);
        return editorJson.ToString(Formatting.None);
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
        ImageCropperConfiguration? configuration = _dataTypeService.GetDataType(propertyType.DataTypeId)
            ?.ConfigurationAs<ImageCropperConfiguration>();
        ImageCropperConfiguration.Crop[] crops = configuration?.Crops ?? Array.Empty<ImageCropperConfiguration.Crop>();

        return JsonConvert.SerializeObject(
            new { src = val, crops },
            new JsonSerializerSettings { Formatting = Formatting.None, NullValueHandling = NullValueHandling.Ignore });
    }

    private string? ProcessFile(ContentPropertyFile file, Guid cuid, Guid puid)
    {
        // process the file
        // no file, invalid file, reject change
        if (UploadFileTypeValidator.IsValidFileExtension(file.FileName, _contentSettings) == false)
        {
            return null;
        }

        // get the filepath
        // in case we are using the old path scheme, try to re-use numbers (bah...)
        var filepath = _mediaFileManager.GetMediaPath(file.FileName, cuid, puid); // fs-relative path

        using (FileStream filestream = File.OpenRead(file.TempFilePath))
        {
            // TODO: Here it would make sense to do the auto-fill properties stuff but the API doesn't allow us to do that right
            // since we'd need to be able to return values for other properties from these methods
            _mediaFileManager.FileSystem.AddFile(filepath, filestream, true); // must overwrite!
        }

        return filepath;
    }
}
