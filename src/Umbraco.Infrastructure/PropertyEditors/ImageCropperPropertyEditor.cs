// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
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
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors
{
    /// <summary>
    /// Represents an image cropper property editor.
    /// </summary>
    [DataEditor(
        Constants.PropertyEditors.Aliases.ImageCropper,
        "Image Cropper",
        "imagecropper",
        ValueType = ValueTypes.Json,
        HideLabel = false,
        Group = Constants.PropertyEditors.Groups.Media,
        Icon = "icon-crop")]
    public class ImageCropperPropertyEditor : DataEditor, IMediaUrlGenerator,
        INotificationHandler<ContentCopiedNotification>, INotificationHandler<ContentDeletedNotification>,
        INotificationHandler<MediaDeletedNotification>, INotificationHandler<MediaSavingNotification>,
        INotificationHandler<MemberDeletedNotification>
    {
        private readonly MediaFileManager _mediaFileManager;
        private readonly ContentSettings _contentSettings;
        private readonly IDataTypeService _dataTypeService;
        private readonly IIOHelper _ioHelper;
        private readonly UploadAutoFillProperties _autoFillProperties;
        private readonly ILogger<ImageCropperPropertyEditor> _logger;
        private readonly IContentService _contentService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageCropperPropertyEditor"/> class.
        /// </summary>
        public ImageCropperPropertyEditor(
            IDataValueEditorFactory dataValueEditorFactory,
            ILoggerFactory loggerFactory,
            MediaFileManager mediaFileManager,
            IOptions<ContentSettings> contentSettings,
            IDataTypeService dataTypeService,
            IIOHelper ioHelper,
            UploadAutoFillProperties uploadAutoFillProperties,
            IContentService contentService)
            : base(dataValueEditorFactory)
        {
            _mediaFileManager = mediaFileManager ?? throw new ArgumentNullException(nameof(mediaFileManager));
            _contentSettings = contentSettings.Value ?? throw new ArgumentNullException(nameof(contentSettings));
            _dataTypeService = dataTypeService ?? throw new ArgumentNullException(nameof(dataTypeService));
            _ioHelper = ioHelper ?? throw new ArgumentNullException(nameof(ioHelper));
            _autoFillProperties = uploadAutoFillProperties ?? throw new ArgumentNullException(nameof(uploadAutoFillProperties));
            _contentService = contentService;
            _logger = loggerFactory.CreateLogger<ImageCropperPropertyEditor>();
        }

        public bool TryGetMediaPath(string alias, object value, out string mediaPath)
        {
            if (alias == Alias)
            {
                mediaPath = GetFileSrcFromPropertyValue(value, out _, false);
                return true;
            }
            mediaPath = null;
            return false;
        }

        /// <summary>
        /// Creates the corresponding property value editor.
        /// </summary>
        /// <returns>The corresponding property value editor.</returns>
        protected override IDataValueEditor CreateValueEditor() => DataValueEditorFactory.Create<ImageCropperPropertyValueEditor>(Attribute);

        /// <summary>
        /// Creates the corresponding preValue editor.
        /// </summary>
        /// <returns>The corresponding preValue editor.</returns>
        protected override IConfigurationEditor CreateConfigurationEditor() => new ImageCropperConfigurationEditor(_ioHelper);

        /// <summary>
        /// Gets a value indicating whether a property is an image cropper field.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>A value indicating whether a property is an image cropper field, and (optionally) has a non-empty value.</returns>
        private static bool IsCropperField(IProperty property)
        {
            return property.PropertyType.PropertyEditorAlias == Constants.PropertyEditors.Aliases.ImageCropper;
        }

        /// <summary>
        /// Parses the property value into a json object.
        /// </summary>
        /// <param name="value">The property value.</param>
        /// <param name="writeLog">A value indicating whether to log the error.</param>
        /// <returns>The json object corresponding to the property value.</returns>
        /// <remarks>In case of an error, optionally logs the error and returns null.</remarks>
        private JObject GetJObject(string value, bool writeLog)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            try
            {
                return JsonConvert.DeserializeObject<JObject>(value);
            }
            catch (Exception ex)
            {
                if (writeLog)
                    _logger.LogError(ex, "Could not parse image cropper value '{Json}'", value);
                return null;
            }
        }

        /// <summary>
        /// The paths to all image cropper property files contained within a collection of content entities
        /// </summary>
        /// <param name="entities"></param>
        private IEnumerable<string> ContainedFilePaths(IEnumerable<IContentBase> entities) => entities
            .SelectMany(x => x.Properties)
            .Where(IsCropperField)
            .SelectMany(GetFilePathsFromPropertyValues)
            .Distinct();

        /// <summary>
        /// Look through all property values stored against the property and resolve any file paths stored
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        private IEnumerable<string> GetFilePathsFromPropertyValues(IProperty prop)
        {
            //parses out the src from a json string

            foreach (var propertyValue in prop.Values)
            {
                //check if the published value contains data and return it
                var src = GetFileSrcFromPropertyValue(propertyValue.PublishedValue, out var _);
                if (src != null) yield return _mediaFileManager.FileSystem.GetRelativePath(src);

                //check if the edited value contains data and return it
                src = GetFileSrcFromPropertyValue(propertyValue.EditedValue, out var _);
                if (src != null) yield return _mediaFileManager.FileSystem.GetRelativePath(src);
            }
        }

        /// <summary>
        /// Returns the "src" property from the json structure if the value is formatted correctly
        /// </summary>
        /// <param name="propVal"></param>
        /// <param name="deserializedValue">The deserialized <see cref="JObject"/> value</param>
        /// <param name="relative">Should the path returned be the application relative path</param>
        /// <returns></returns>
        private string GetFileSrcFromPropertyValue(object propVal, out JObject deserializedValue, bool relative = true)
        {
            deserializedValue = null;
            if (propVal == null || !(propVal is string str)) return null;
            if (!str.DetectIsJson()) return null;
            deserializedValue = GetJObject(str, true);
            if (deserializedValue?["src"] == null) return null;
            var src = deserializedValue["src"].Value<string>();
            return relative ? _mediaFileManager.FileSystem.GetRelativePath(src) : src;
        }

        /// <summary>
        /// After a content has been copied, also copy uploaded files.
        /// </summary>
        public void Handle(ContentCopiedNotification notification)
        {
            // get the image cropper field properties
            var properties = notification.Original.Properties.Where(IsCropperField);

            // copy files
            var isUpdated = false;
            foreach (var property in properties)
            {
                //copy each of the property values (variants, segments) to the destination by using the edited value
                foreach (var propertyValue in property.Values)
                {
                    var propVal = property.GetValue(propertyValue.Culture, propertyValue.Segment);
                    var src = GetFileSrcFromPropertyValue(propVal, out var jo);
                    if (src == null)
                    {
                        continue;
                    }
                    var sourcePath = _mediaFileManager.FileSystem.GetRelativePath(src);
                    var copyPath = _mediaFileManager.CopyFile(notification.Copy, property.PropertyType, sourcePath);
                    jo["src"] = _mediaFileManager.FileSystem.GetUrl(copyPath);
                    notification.Copy.SetValue(property.Alias, jo.ToString(), propertyValue.Culture, propertyValue.Segment);
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

        public void Handle(MemberDeletedNotification notification) => DeleteContainedFiles(notification.DeletedEntities);

        private void DeleteContainedFiles(IEnumerable<IContentBase> deletedEntities)
        {
            var filePathsToDelete = ContainedFilePaths(deletedEntities);
            _mediaFileManager.DeleteMediaFiles(filePathsToDelete);
        }

        public void Handle(MediaSavingNotification notification)
        {
            foreach (var entity in notification.SavedEntities)
            {
                AutoFillProperties(entity);
            }
        }

        /// <summary>
        /// Auto-fill properties (or clear).
        /// </summary>
        private void AutoFillProperties(IContentBase model)
        {
            var properties = model.Properties.Where(IsCropperField);

            foreach (var property in properties)
            {
                var autoFillConfig = _contentSettings.GetConfig(property.Alias);
                if (autoFillConfig == null) continue;

                foreach (var pvalue in property.Values)
                {
                    var svalue = property.GetValue(pvalue.Culture, pvalue.Segment) as string;
                    if (string.IsNullOrWhiteSpace(svalue))
                    {
                        _autoFillProperties.Reset(model, autoFillConfig, pvalue.Culture, pvalue.Segment);
                    }
                    else
                    {
                        var jo = GetJObject(svalue, false);
                        string src;
                        if (jo == null)
                        {
                            // so we have a non-empty string value that cannot be parsed into a json object
                            // see http://issues.umbraco.org/issue/U4-4756
                            // it can happen when an image is uploaded via the folder browser, in which case
                            // the property value will be the file source eg '/media/23454/hello.jpg' and we
                            // are fixing that anomaly here - does not make any sense at all but... bah...

                            var dt = _dataTypeService.GetDataType(property.PropertyType.DataTypeId);
                            var config = dt?.ConfigurationAs<ImageCropperConfiguration>();
                            src = svalue;
                            var json = new
                            {
                                src = svalue,
                                crops = config == null ? Array.Empty<ImageCropperConfiguration.Crop>() : config.Crops
                            };

                            property.SetValue(JsonConvert.SerializeObject(json), pvalue.Culture, pvalue.Segment);
                        }
                        else
                        {
                            src = jo["src"]?.Value<string>();
                        }

                        if (src == null)
                            _autoFillProperties.Reset(model, autoFillConfig, pvalue.Culture, pvalue.Segment);
                        else
                            _autoFillProperties.Populate(model, autoFillConfig, _mediaFileManager.FileSystem.GetRelativePath(src), pvalue.Culture, pvalue.Segment);
                    }
                }
            }
        }
    }
}
