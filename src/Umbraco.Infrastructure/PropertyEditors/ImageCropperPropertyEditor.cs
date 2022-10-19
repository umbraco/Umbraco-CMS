// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents an image cropper property editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.ImageCropper,
    "Image Cropper",
    "imagecropper",
    ValueType = ValueTypes.Json,
    HideLabel = false,
    Group = Constants.PropertyEditors.Groups.Media,
    Icon = "icon-crop",
    ValueEditorIsReusable = true)]
public class ImageCropperPropertyEditor : DataEditor, IMediaUrlGenerator,
    INotificationHandler<ContentCopiedNotification>, INotificationHandler<ContentDeletedNotification>,
    INotificationHandler<MediaDeletedNotification>, INotificationHandler<MediaSavingNotification>,
    INotificationHandler<MemberDeletedNotification>
{
    private readonly UploadAutoFillProperties _autoFillProperties;
    private readonly IContentService _contentService;
    private readonly IDataTypeService _dataTypeService;
    private readonly IEditorConfigurationParser _editorConfigurationParser;
    private readonly IIOHelper _ioHelper;
    private readonly ILogger<ImageCropperPropertyEditor> _logger;
    private readonly MediaFileManager _mediaFileManager;
    private ContentSettings _contentSettings;

    // Scheduled for removal in v12
    [Obsolete("Please use constructor that takes an IEditorConfigurationParser instead")]
    public ImageCropperPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        ILoggerFactory loggerFactory,
        MediaFileManager mediaFileManager,
        IOptionsMonitor<ContentSettings> contentSettings,
        IDataTypeService dataTypeService,
        IIOHelper ioHelper,
        UploadAutoFillProperties uploadAutoFillProperties,
        IContentService contentService)
        : this(
            dataValueEditorFactory,
            loggerFactory,
            mediaFileManager,
            contentSettings,
            dataTypeService,
            ioHelper,
            uploadAutoFillProperties,
            contentService,
            StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ImageCropperPropertyEditor" /> class.
    /// </summary>
    public ImageCropperPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        ILoggerFactory loggerFactory,
        MediaFileManager mediaFileManager,
        IOptionsMonitor<ContentSettings> contentSettings,
        IDataTypeService dataTypeService,
        IIOHelper ioHelper,
        UploadAutoFillProperties uploadAutoFillProperties,
        IContentService contentService,
        IEditorConfigurationParser editorConfigurationParser)
        : base(dataValueEditorFactory)
    {
        _mediaFileManager = mediaFileManager ?? throw new ArgumentNullException(nameof(mediaFileManager));
        _contentSettings = contentSettings.CurrentValue ?? throw new ArgumentNullException(nameof(contentSettings));
        _dataTypeService = dataTypeService ?? throw new ArgumentNullException(nameof(dataTypeService));
        _ioHelper = ioHelper ?? throw new ArgumentNullException(nameof(ioHelper));
        _autoFillProperties =
            uploadAutoFillProperties ?? throw new ArgumentNullException(nameof(uploadAutoFillProperties));
        _contentService = contentService;
        _editorConfigurationParser = editorConfigurationParser;
        _logger = loggerFactory.CreateLogger<ImageCropperPropertyEditor>();

        contentSettings.OnChange(x => _contentSettings = x);
        SupportsReadOnly = true;
    }

    public bool TryGetMediaPath(string? propertyEditorAlias, object? value, out string? mediaPath)
    {
        if (propertyEditorAlias == Alias &&
            GetFileSrcFromPropertyValue(value, out _, false) is var mediaPathValue &&
            !string.IsNullOrWhiteSpace(mediaPathValue))
        {
            mediaPath = mediaPathValue;
            return true;
        }

        mediaPath = null;
        return false;
    }

    /// <summary>
    ///     After a content has been copied, also copy uploaded files.
    /// </summary>
    public void Handle(ContentCopiedNotification notification)
    {
        // get the image cropper field properties
        IEnumerable<IProperty> properties = notification.Original.Properties.Where(IsCropperField);

        // copy files
        var isUpdated = false;
        foreach (IProperty property in properties)
        {
            // copy each of the property values (variants, segments) to the destination by using the edited value
            foreach (IPropertyValue propertyValue in property.Values)
            {
                var propVal = property.GetValue(propertyValue.Culture, propertyValue.Segment);
                var src = GetFileSrcFromPropertyValue(propVal, out JObject? jo);
                if (src == null)
                {
                    continue;
                }

                var sourcePath = _mediaFileManager.FileSystem.GetRelativePath(src);
                var copyPath = _mediaFileManager.CopyFile(notification.Copy, property.PropertyType, sourcePath);
                jo!["src"] = _mediaFileManager.FileSystem.GetUrl(copyPath);
                notification.Copy.SetValue(property.Alias, jo.ToString(Formatting.None), propertyValue.Culture,
                    propertyValue.Segment);
                isUpdated = true;
            }
        }

        // if updated, re-save the copy with the updated value
        if (isUpdated)
        {
            _contentService.Save(notification.Copy);
        }
    }

    public void Handle(ContentDeletedNotification notification) => DeleteContainedFiles(notification.DeletedEntities);

    public void Handle(MediaDeletedNotification notification) => DeleteContainedFiles(notification.DeletedEntities);

    public void Handle(MediaSavingNotification notification)
    {
        foreach (IMedia entity in notification.SavedEntities)
        {
            AutoFillProperties(entity);
        }
    }

    public void Handle(MemberDeletedNotification notification) => DeleteContainedFiles(notification.DeletedEntities);

    /// <summary>
    ///     Creates the corresponding property value editor.
    /// </summary>
    /// <returns>The corresponding property value editor.</returns>
    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<ImageCropperPropertyValueEditor>(Attribute!);

    /// <summary>
    ///     Creates the corresponding preValue editor.
    /// </summary>
    /// <returns>The corresponding preValue editor.</returns>
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new ImageCropperConfigurationEditor(_ioHelper, _editorConfigurationParser);

    /// <summary>
    ///     Gets a value indicating whether a property is an image cropper field.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>
    ///     <c>true</c> if the specified property is an image cropper field; otherwise, <c>false</c>.
    /// </returns>
    private static bool IsCropperField(IProperty property) => property.PropertyType.PropertyEditorAlias ==
                                                              Constants.PropertyEditors.Aliases.ImageCropper;

    /// <summary>
    ///     Parses the property value into a json object.
    /// </summary>
    /// <param name="value">The property value.</param>
    /// <param name="writeLog">A value indicating whether to log the error.</param>
    /// <returns>The json object corresponding to the property value.</returns>
    /// <remarks>In case of an error, optionally logs the error and returns null.</remarks>
    private JObject? GetJObject(string value, bool writeLog)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        try
        {
            return JsonConvert.DeserializeObject<JObject>(value);
        }
        catch (Exception ex)
        {
            if (writeLog)
            {
                _logger.LogError(ex, "Could not parse image cropper value '{Json}'", value);
            }

            return null;
        }
    }

    /// <summary>
    ///     The paths to all image cropper property files contained within a collection of content entities
    /// </summary>
    /// <param name="entities"></param>
    private IEnumerable<string> ContainedFilePaths(IEnumerable<IContentBase> entities) => entities
        .SelectMany(x => x.Properties)
        .Where(IsCropperField)
        .SelectMany(GetFilePathsFromPropertyValues)
        .Distinct();

    /// <summary>
    ///     Look through all property values stored against the property and resolve any file paths stored
    /// </summary>
    /// <param name="prop"></param>
    /// <returns></returns>
    private IEnumerable<string> GetFilePathsFromPropertyValues(IProperty prop)
    {
        // parses out the src from a json string
        foreach (IPropertyValue propertyValue in prop.Values)
        {
            // check if the published value contains data and return it
            var src = GetFileSrcFromPropertyValue(propertyValue.PublishedValue, out JObject? _);
            if (src != null)
            {
                yield return _mediaFileManager.FileSystem.GetRelativePath(src);
            }

            // check if the edited value contains data and return it
            src = GetFileSrcFromPropertyValue(propertyValue.EditedValue, out JObject? _);
            if (src != null)
            {
                yield return _mediaFileManager.FileSystem.GetRelativePath(src);
            }
        }
    }

    /// <summary>
    ///     Returns the "src" property from the json structure if the value is formatted correctly
    /// </summary>
    /// <param name="propVal"></param>
    /// <param name="deserializedValue">The deserialized <see cref="JObject" /> value</param>
    /// <param name="relative">Should the path returned be the application relative path</param>
    /// <returns></returns>
    private string? GetFileSrcFromPropertyValue(object? propVal, out JObject? deserializedValue, bool relative = true)
    {
        deserializedValue = null;
        if (propVal == null || !(propVal is string str))
        {
            return null;
        }

        if (!str.DetectIsJson())
        {
            // Assume the value is a plain string with the file path
            deserializedValue = new JObject { { "src", str } };
        }
        else
        {
            deserializedValue = GetJObject(str, true);
        }

        if (deserializedValue?["src"] == null)
        {
            return null;
        }

        var src = deserializedValue["src"]!.Value<string>();

        return relative ? _mediaFileManager.FileSystem.GetRelativePath(src!) : src;
    }

    private void DeleteContainedFiles(IEnumerable<IContentBase> deletedEntities)
    {
        IEnumerable<string> filePathsToDelete = ContainedFilePaths(deletedEntities);
        _mediaFileManager.DeleteMediaFiles(filePathsToDelete);
    }

    /// <summary>
    ///     Auto-fill properties (or clear).
    /// </summary>
    private void AutoFillProperties(IContentBase model)
    {
        IEnumerable<IProperty> properties = model.Properties.Where(IsCropperField);

        foreach (IProperty property in properties)
        {
            ImagingAutoFillUploadField? autoFillConfig = _contentSettings.GetConfig(property.Alias);
            if (autoFillConfig == null)
            {
                continue;
            }

            foreach (IPropertyValue pvalue in property.Values)
            {
                var svalue = property.GetValue(pvalue.Culture, pvalue.Segment) as string;
                if (string.IsNullOrWhiteSpace(svalue))
                {
                    _autoFillProperties.Reset(model, autoFillConfig, pvalue.Culture, pvalue.Segment);
                }
                else
                {
                    JObject? jo = GetJObject(svalue, false);
                    string? src;
                    if (jo == null)
                    {
                        // so we have a non-empty string value that cannot be parsed into a json object
                        // see http://issues.umbraco.org/issue/U4-4756
                        // it can happen when an image is uploaded via the folder browser, in which case
                        // the property value will be the file source eg '/media/23454/hello.jpg' and we
                        // are fixing that anomaly here - does not make any sense at all but... bah...
                        src = svalue;

                        property.SetValue(
                            JsonConvert.SerializeObject(new { src = svalue }, Formatting.None),
                            pvalue.Culture, pvalue.Segment);
                    }
                    else
                    {
                        src = jo["src"]?.Value<string>();
                    }

                    if (src == null)
                    {
                        _autoFillProperties.Reset(model, autoFillConfig, pvalue.Culture, pvalue.Segment);
                    }
                    else
                    {
                        _autoFillProperties.Populate(model, autoFillConfig,
                            _mediaFileManager.FileSystem.GetRelativePath(src), pvalue.Culture, pvalue.Segment);
                    }
                }
            }
        }
    }
}
