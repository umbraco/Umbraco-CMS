using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Media;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents an image cropper property editor.
    /// </summary>
    [DataEditor(Constants.PropertyEditors.Aliases.ImageCropper, "Image Cropper", "imagecropper", ValueType = ValueTypes.Json, HideLabel = false, Group="media", Icon="icon-crop")]
    public class ImageCropperPropertyEditor : DataEditor
    {
        private readonly MediaFileSystem _mediaFileSystem;
        private readonly UploadAutoFillProperties _autoFillProperties;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageCropperPropertyEditor"/> class.
        /// </summary>
        public ImageCropperPropertyEditor(ILogger logger, MediaFileSystem mediaFileSystem, IContentSection contentSettings)
            : base(logger)
        {
            _mediaFileSystem = mediaFileSystem ?? throw new ArgumentNullException(nameof(mediaFileSystem));
            var contentSettings1 = contentSettings ?? throw new ArgumentNullException(nameof(contentSettings));

            _autoFillProperties = new UploadAutoFillProperties(_mediaFileSystem, Logger, contentSettings1);
        }

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
        /// <param name="ensureValue">A value indicating whether to check that the property has a non-empty value.</param>
        /// <returns>A value indicating whether a property is an image cropper field, and (optionaly) has a non-empty value.</returns>
        private static bool IsCropperField(Property property, bool ensureValue)
        {
            if (property.PropertyType.PropertyEditorAlias != Constants.PropertyEditors.Aliases.ImageCropper)
                return false;
            if (ensureValue == false)
                return true;
            var val = property.GetValue();
            return val is string s && string.IsNullOrWhiteSpace(s) == false;
        }

        /// <summary>
        /// Parses the property value into a json object.
        /// </summary>
        /// <param name="value">The property value.</param>
        /// <param name="writeLog">A value indicating whether to log the error.</param>
        /// <returns>The json object corresponding to the property value.</returns>
        /// <remarks>In case of an error, optionaly logs the error and returns null.</remarks>
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
                    Logger.Error<ImageCropperPropertyEditor>($"Could not parse image cropper value \"{value}\"", ex);
                return null;
            }
        }

        /// <summary>
        /// Ensures any files associated are removed
        /// </summary>
        /// <param name="allPropertyData"></param>
        internal IEnumerable<string> ServiceEmptiedRecycleBin(Dictionary<int, IEnumerable<Property>> allPropertyData)
        {
            return allPropertyData.SelectMany(x => x.Value)
                .Where(x => IsCropperField(x, true)).Select(x =>
                {
                    var jo = GetJObject((string) x.GetValue(), true);
                    if (jo?["src"] == null) return null;
                    var src = jo["src"].Value<string>();
                    return string.IsNullOrWhiteSpace(src) ? null : _mediaFileSystem.GetRelativePath(src);
                }).WhereNotNull();
        }

        /// <summary>
        /// Ensures any files associated are removed
        /// </summary>
        /// <param name="deletedEntities"></param>
        internal IEnumerable<string> ServiceDeleted(IEnumerable<ContentBase> deletedEntities)
        {
            return deletedEntities.SelectMany(x => x.Properties)
                .Where(x => IsCropperField(x, true)).Select(x =>
                {
                    var jo = GetJObject((string) x.GetValue(), true);
                    if (jo?["src"] == null) return null;
                    var src = jo["src"].Value<string>();
                    return string.IsNullOrWhiteSpace(src) ? null : _mediaFileSystem.GetRelativePath(src);
                }).WhereNotNull();
        }

        /// <summary>
        /// After a content has been copied, also copy uploaded files.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The event arguments.</param>
        public void ContentServiceCopied(IContentService sender, Core.Events.CopyEventArgs<IContent> args)
        {
            // get the image cropper field properties with a value
            var properties = args.Original.Properties.Where(x => IsCropperField(x, true));

            // copy files
            var isUpdated = false;
            foreach (var property in properties)
            {
                var jo = GetJObject((string) property.GetValue(), true);

                var src = jo?["src"]?.Value<string>();
                if (string.IsNullOrWhiteSpace(src)) continue;

                var sourcePath = _mediaFileSystem.GetRelativePath(src);
                var copyPath = _mediaFileSystem.CopyFile(args.Copy, property.PropertyType, sourcePath);
                jo["src"] = _mediaFileSystem.GetUrl(copyPath);
                args.Copy.SetValue(property.Alias, jo.ToString());
                isUpdated = true;
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
            var properties = model.Properties.Where(x => IsCropperField(x, false));

            foreach (var property in properties)
            {
                var autoFillConfig = _autoFillProperties.GetConfig(property.Alias);
                if (autoFillConfig == null) continue;

                foreach (var pvalue in property.Values)
                {
                    var svalue = property.GetValue(pvalue.LanguageId, pvalue.Segment) as string;
                    if (string.IsNullOrWhiteSpace(svalue))
                    {
                        _autoFillProperties.Reset(model, autoFillConfig, pvalue.LanguageId, pvalue.Segment);
                        continue;
                    }

                    // FIXME VERY TEMP
                    // we should kill all auto-fill properties
                    // BUT that being said what would be the right way to do this?
                    /*
                    var v = JsonConvert.DeserializeObject<ImageCropperValue>()

                    var jo = GetJObject(svalue, false);
                    string src;
                    if (jo == null)
                    {
                        // so we have a non-empty string value that cannot be parsed into a json object
                        // see http://issues.umbraco.org/issue/U4-4756
                        // it can happen when an image is uploaded via the folder browser, in which case
                        // the property value will be the file source eg '/media/23454/hello.jpg' and we
                        // are fixing that anomaly here - does not make any sense at all but... bah...
                        var config = _dataTypeService
                            .GetPreValuesByDataTypeId(property.PropertyType.DataTypeDefinitionId).FirstOrDefault();
                        var crops = string.IsNullOrWhiteSpace(config) ? "[]" : config;
                        src = svalue;
                        property.SetValue("{\"src\": \"" + svalue + "\", \"crops\": " + crops + "}");
                    }
                    else
                    {
                        src = jo["src"]?.Value<string>();
                    }

                    if (src == null)
                        _autoFillProperties.Reset(model, autoFillConfig, pvalue.LanguageId, pvalue.Segment);
                    else
                        _autoFillProperties.Populate(model, autoFillConfig, _mediaFileSystem.GetRelativePath(src), pvalue.LanguageId, pvalue.Segment);
                    */
                }
            }
        }
    }
}
