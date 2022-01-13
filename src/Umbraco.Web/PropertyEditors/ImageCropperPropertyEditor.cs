using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Media;

namespace Umbraco.Web.PropertyEditors
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
    public class ImageCropperPropertyEditor : DataEditor, IDataEditorWithMediaPath
    {
        private readonly IMediaFileSystem _mediaFileSystem;
        private readonly IContentSection _contentSettings;
        private readonly IDataTypeService _dataTypeService;
        private readonly UploadAutoFillProperties _autoFillProperties;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageCropperPropertyEditor"/> class.
        /// </summary>
        public ImageCropperPropertyEditor(ILogger logger, IMediaFileSystem mediaFileSystem, IContentSection contentSettings, IDataTypeService dataTypeService)
            : base(logger)
        {
            _mediaFileSystem = mediaFileSystem ?? throw new ArgumentNullException(nameof(mediaFileSystem));
            _contentSettings = contentSettings ?? throw new ArgumentNullException(nameof(contentSettings));
            _dataTypeService = dataTypeService;

            // TODO: inject?
            _autoFillProperties = new UploadAutoFillProperties(_mediaFileSystem, logger, _contentSettings);
        }

        public string GetMediaPath(object value) => GetFileSrcFromPropertyValue(value, out _, false);

        /// <summary>
        /// Creates the corresponding property value editor.
        /// </summary>
        /// <returns>The corresponding property value editor.</returns>
        protected override IDataValueEditor CreateValueEditor() => new ImageCropperPropertyValueEditor(Attribute, Logger, _mediaFileSystem);

        /// <summary>
        /// Creates the corresponding preValue editor.
        /// </summary>
        /// <returns>The corresponding preValue editor.</returns>
        protected override IConfigurationEditor CreateConfigurationEditor() => new ImageCropperConfigurationEditor();

        /// <summary>
        /// Gets a value indicating whether a property is an image cropper field.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>A value indicating whether a property is an image cropper field, and (optionally) has a non-empty value.</returns>
        private static bool IsCropperField(Property property)
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
                    Logger.Error<ImageCropperPropertyEditor, string>(ex, "Could not parse image cropper value '{Json}'", value);
                return null;
            }
        }

        /// <summary>
        /// Ensures any files associated are removed
        /// </summary>
        /// <param name="deletedEntities"></param>
        internal IEnumerable<string> ServiceDeleted(IEnumerable<ContentBase> deletedEntities)
        {
            return deletedEntities.SelectMany(x => x.Properties)
                .Where(IsCropperField)
                .SelectMany(GetFilePathsFromPropertyValues)
                .Distinct();
        }

        /// <summary>
        /// Look through all property values stored against the property and resolve any file paths stored
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        private IEnumerable<string> GetFilePathsFromPropertyValues(Property prop)
        {
            //parses out the src from a json string

            foreach (var propertyValue in prop.Values)
            {
                //check if the published value contains data and return it
                var src = GetFileSrcFromPropertyValue(propertyValue.PublishedValue, out var _);
                if (src != null) yield return _mediaFileSystem.GetRelativePath(src);

                //check if the edited value contains data and return it
                src = GetFileSrcFromPropertyValue(propertyValue.EditedValue, out var _);
                if (src != null) yield return _mediaFileSystem.GetRelativePath(src);
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
            return relative ? _mediaFileSystem.GetRelativePath(src) : src;
        }

        /// <summary>
        /// After a content has been copied, also copy uploaded files.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The event arguments.</param>
        public void ContentServiceCopied(IContentService sender, Core.Events.CopyEventArgs<IContent> args)
        {
            // get the image cropper field properties
            var properties = args.Original.Properties.Where(IsCropperField);

            // copy files
            var isUpdated = false;
            foreach (var property in properties)
            {
                //copy each of the property values (variants, segments) to the destination by using the edited value
                foreach (var propertyValue in property.Values)
                {
                    var propVal = property.GetValue(propertyValue.Culture, propertyValue.Segment);
                    var src = GetFileSrcFromPropertyValue(propVal, out var jo);
                    if (src == null) continue;
                    var sourcePath = _mediaFileSystem.GetRelativePath(src);
                    var copyPath = _mediaFileSystem.CopyFile(args.Copy, property.PropertyType, sourcePath);
                    jo["src"] = _mediaFileSystem.GetUrl(copyPath);
                    args.Copy.SetValue(property.Alias, jo.ToString(Formatting.None), propertyValue.Culture, propertyValue.Segment);
                    isUpdated = true;
                }
            }
            // if updated, re-save the copy with the updated value
            if (isUpdated)
                sender.Save(args.Copy);
        }

        /// <summary>
        /// After a media has been created, auto-fill the properties.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The event arguments.</param>
        public void MediaServiceCreated(IMediaService sender, Core.Events.NewEventArgs<IMedia> args)
        {
            AutoFillProperties(args.Entity);
        }

        /// <summary>
        /// After a media has been saved, auto-fill the properties.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The event arguments.</param>
        public void MediaServiceSaving(IMediaService sender, Core.Events.SaveEventArgs<IMedia> args)
        {
            foreach (var entity in args.SavedEntities)
                AutoFillProperties(entity);
        }

        /// <summary>
        /// After a content item has been saved, auto-fill the properties.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The event arguments.</param>
        public void ContentServiceSaving(IContentService sender, Core.Events.SaveEventArgs<IContent> args)
        {
            foreach (var entity in args.SavedEntities)
                AutoFillProperties(entity);
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
                            src = svalue;

                            property.SetValue(JsonConvert.SerializeObject(new
                            {
                                src = svalue
                            }, Formatting.None), pvalue.Culture, pvalue.Segment);
                        }
                        else
                        {
                            src = jo["src"]?.Value<string>();
                        }

                        if (src == null)
                            _autoFillProperties.Reset(model, autoFillConfig, pvalue.Culture, pvalue.Segment);
                        else
                            _autoFillProperties.Populate(model, autoFillConfig, _mediaFileSystem.GetRelativePath(src), pvalue.Culture, pvalue.Segment);
                    }
                }
            }
        }
    }
}
