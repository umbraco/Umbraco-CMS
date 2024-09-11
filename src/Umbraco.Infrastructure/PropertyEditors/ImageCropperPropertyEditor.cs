// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents an image cropper property editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.ImageCropper,
    ValueType = ValueTypes.Json,
    ValueEditorIsReusable = true)]
public class ImageCropperPropertyEditor : DataEditor, IMediaUrlGenerator,
    INotificationHandler<ContentCopiedNotification>, INotificationHandler<ContentDeletedNotification>,
    INotificationHandler<MediaDeletedNotification>, INotificationHandler<MediaSavingNotification>,
    INotificationHandler<MemberDeletedNotification>
{
    private readonly UploadAutoFillProperties _autoFillProperties;
    private readonly IContentService _contentService;
    private readonly IIOHelper _ioHelper;
    private readonly ILogger<ImageCropperPropertyEditor> _logger;
    private readonly MediaFileManager _mediaFileManager;
    private ContentSettings _contentSettings;
    private readonly IJsonSerializer _jsonSerializer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ImageCropperPropertyEditor" /> class.
    /// </summary>
    public ImageCropperPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        ILoggerFactory loggerFactory,
        MediaFileManager mediaFileManager,
        IOptionsMonitor<ContentSettings> contentSettings,
        IIOHelper ioHelper,
        UploadAutoFillProperties uploadAutoFillProperties,
        IContentService contentService,
        IJsonSerializer jsonSerializer)
        : base(dataValueEditorFactory)
    {
        _mediaFileManager = mediaFileManager ?? throw new ArgumentNullException(nameof(mediaFileManager));
        _contentSettings = contentSettings.CurrentValue ?? throw new ArgumentNullException(nameof(contentSettings));
        _ioHelper = ioHelper ?? throw new ArgumentNullException(nameof(ioHelper));
        _autoFillProperties =
            uploadAutoFillProperties ?? throw new ArgumentNullException(nameof(uploadAutoFillProperties));
        _contentService = contentService;
        _jsonSerializer = jsonSerializer;
        _logger = loggerFactory.CreateLogger<ImageCropperPropertyEditor>();

        contentSettings.OnChange(x => _contentSettings = x);
        SupportsReadOnly = true;
    }

    public override IPropertyIndexValueFactory PropertyIndexValueFactory { get; } = new NoopPropertyIndexValueFactory();

    public bool TryGetMediaPath(string? propertyEditorAlias, object? value, out string? mediaPath)
    {
        if (propertyEditorAlias == Alias &&
            GetFileSrcFromPropertyValue(value, false) is var mediaPathValue &&
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
                var sourcePath = GetFileSrcFromPropertyValue(propVal, relative: true);
                if (sourcePath.IsNullOrWhiteSpace())
                {
                    continue;
                }

                var copyPath = _mediaFileManager.CopyFile(notification.Copy, property.PropertyType, sourcePath);
                ImageCropperValue? newValue = (propVal is string stringValue && stringValue.DetectIsJson()
                    ? _jsonSerializer.Deserialize<ImageCropperValue>(stringValue)
                    : null) ?? new ImageCropperValue();
                newValue.Src = _mediaFileManager.FileSystem.GetUrl(copyPath);
                notification.Copy.SetValue(property.Alias,  _jsonSerializer.Serialize(newValue), propertyValue.Culture,
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
        new ImageCropperConfigurationEditor(_ioHelper);

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
            var src = GetFileSrcFromPropertyValue(propertyValue.PublishedValue);
            if (src != null)
            {
                yield return _mediaFileManager.FileSystem.GetRelativePath(src);
            }

            // check if the edited value contains data and return it
            src = GetFileSrcFromPropertyValue(propertyValue.EditedValue);
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
    /// <param name="relative">Should the path returned be the application relative path</param>
    /// <returns></returns>
    private string? GetFileSrcFromPropertyValue(object? propVal, bool relative = true)
    {
        if (propVal is not string stringValue)
        {
            return null;
        }

        string? source = null;

        if (!stringValue.DetectIsJson())
        {
            // Assume the value is a plain string with the file path
            source = stringValue;
        }
        else
        {
            try
            {
                source = _jsonSerializer.Deserialize<LightWeightImageCropperValue>(stringValue)?.Src;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not parse image cropper value '{Json}'", stringValue);
            }
        }

        if (source.IsNullOrWhiteSpace())
        {
            return null;
        }

        return relative ? _mediaFileManager.FileSystem.GetRelativePath(source) : source;
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
                var value = property.GetValue(pvalue.Culture, pvalue.Segment);
                var source = GetFileSrcFromPropertyValue(property.GetValue(pvalue.Culture, pvalue.Segment));
                if (source.IsNullOrWhiteSpace())
                {
                    _autoFillProperties.Reset(model, autoFillConfig, pvalue.Culture, pvalue.Segment);
                }
                else
                {
                    if (value is string stringValue && stringValue.DetectIsJson() is false)
                    {
                        // so we have a non-empty string value that cannot be parsed into a json object
                        // see http://issues.umbraco.org/issue/U4-4756
                        // it can happen when an image is uploaded via the folder browser, in which case
                        // the property value will be the file source eg '/media/23454/hello.jpg' and we
                        // are fixing that anomaly here - does not make any sense at all but... bah...
                        property.SetValue(
                            _jsonSerializer.Serialize(new LightWeightImageCropperValue { Src = stringValue }),
                            pvalue.Culture, pvalue.Segment);
                    }

                    if (source is null)
                    {
                        _autoFillProperties.Reset(model, autoFillConfig, pvalue.Culture, pvalue.Segment);
                    }
                    else
                    {
                        _autoFillProperties.Populate(model, autoFillConfig, source, pvalue.Culture, pvalue.Segment);
                    }
                }
            }
        }
    }

    // for efficient value deserialization, we don't want to deserialize more than we need to (we don't need crops, focal point etc.)
    private class LightWeightImageCropperValue
    {
        public string? Src { get; set; } = string.Empty;
    }
}
