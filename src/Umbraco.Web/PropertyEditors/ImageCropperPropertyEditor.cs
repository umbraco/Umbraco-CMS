using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Media;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.ImageCropperAlias, "Image Cropper", "imagecropper", ValueType = PropertyEditorValueTypes.Json, HideLabel = false, Group="media", Icon="icon-crop")]
    public class ImageCropperPropertyEditor : PropertyEditor, IApplicationEventHandler
    {
        // preValues
        private IDictionary<string, object> _internalPreValues;

        public override IDictionary<string, object> DefaultPreValues
        {
            get { return _internalPreValues; }
            set { _internalPreValues = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageCropperPropertyEditor"/> class.
        /// </summary>
        public ImageCropperPropertyEditor()
        {
            _internalPreValues = new Dictionary<string, object>
                {
                    {"focalPoint", "{left: 0.5, top: 0.5}"},
                    {"src", ""}
                };
        }

        private static MediaFileSystem MediaFileSystem
        {
            // v8 will get rid of singletons
            get { return FileSystemProviderManager.Current.MediaFileSystem; }
        }

        /// <summary>
        /// Creates the corresponding property value editor.
        /// </summary>
        /// <returns>The corresponding property value editor.</returns>
        protected override PropertyValueEditor CreateValueEditor()
        {
            var baseEditor = base.CreateValueEditor();
            return new ImageCropperPropertyValueEditor(baseEditor, MediaFileSystem);
        }

        /// <summary>
        /// Creates the corresponding preValue editor.
        /// </summary>
        /// <returns>The corresponding preValue editor.</returns>
        protected override PreValueEditor CreatePreValueEditor()
        {
            return new ImageCropperPreValueEditor();
        }

        /// <summary>
        /// Gets a value indicating whether a property is an image cropper field.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="ensureValue">A value indicating whether to check that the property has a non-empty value.</param>
        /// <returns>A value indicating whether a property is an image cropper field, and (optionaly) has a non-empty value.</returns>
        private static bool IsCropperField(Property property, bool ensureValue)
        {
            if (property.PropertyType.PropertyEditorAlias != Constants.PropertyEditors.ImageCropperAlias)
                return false;
            if (ensureValue == false)
                return true;
            return property.Value is string && string.IsNullOrWhiteSpace((string)property.Value) == false;
        }

        /// <summary>
        /// Parses the property value into a json object.
        /// </summary>
        /// <param name="value">The property value.</param>
        /// <param name="writeLog">A value indicating whether to log the error.</param>
        /// <returns>The json object corresponding to the property value.</returns>
        /// <remarks>In case of an error, optionaly logs the error and returns null.</remarks>
        private static JObject GetJObject(string value, bool writeLog)
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
                    LogHelper.Error<ImageCropperPropertyEditor>("Could not parse image cropper value \"" + value + "\"", ex);
                return null;
            }
        }

        /// <summary>
        /// Gets the files that need to be deleted when entities are deleted.
        /// </summary>
        /// <param name="properties">The properties that were deleted.</param>
        static IEnumerable<string> GetFilesToDelete(IEnumerable<Property> properties)
        {
            return properties.Where(x => IsCropperField(x, true)).Select(x =>
            {
                var jo = GetJObject((string) x.Value, true);
                if (jo == null || jo["src"] == null) return null;
                var src = jo["src"].Value<string>();
                return string.IsNullOrWhiteSpace(src) ? null : MediaFileSystem.GetRelativePath(src);
            }).WhereNotNull();
        }

        /// <summary>
        /// After a content has been copied, also copy uploaded files.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The event arguments.</param>
        static void ContentServiceCopied(IContentService sender, Core.Events.CopyEventArgs<IContent> args)
        {
            // get the image cropper field properties with a value
            var properties = args.Original.Properties.Where(x => IsCropperField(x, true));

            // copy files
            var isUpdated = false;
            foreach (var property in properties)
            {
                var jo = GetJObject((string) property.Value, true);
                if (jo == null || jo["src"] == null) continue;

                var src = jo["src"].Value<string>();
                if (string.IsNullOrWhiteSpace(src)) continue;

                var sourcePath = MediaFileSystem.GetRelativePath(src);
                var copyPath = MediaFileSystem.CopyFile(args.Copy, property.PropertyType, sourcePath);
                jo["src"] = MediaFileSystem.GetUrl(copyPath);
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
        static void MediaServiceCreated(IMediaService sender, Core.Events.NewEventArgs<IMedia> args)
        {
            AutoFillProperties(args.Entity);
        }

        /// <summary>
        /// After a media has been saved, auto-fill the properties.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The event arguments.</param>
        static void MediaServiceSaving(IMediaService sender, Core.Events.SaveEventArgs<IMedia> args)
        {
            foreach (var entity in args.SavedEntities)
                AutoFillProperties(entity);
        }

        /// <summary>
        /// After a content item has been saved, auto-fill the properties.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The event arguments.</param>
        static void ContentServiceSaving(IContentService sender, Core.Events.SaveEventArgs<IContent> args)
        {
            foreach (var entity in args.SavedEntities)
                AutoFillProperties(entity);
        }

        /// <summary>
        /// Auto-fill properties (or clear).
        /// </summary>
        /// <param name="content">The content.</param>
        static void AutoFillProperties(IContentBase content)
        {
            var properties = content.Properties.Where(x => IsCropperField(x, false));

            foreach (var property in properties)
            {
                var autoFillConfig = MediaFileSystem.UploadAutoFillProperties.GetConfig(property.Alias);
                if (autoFillConfig == null) continue;

                var svalue = property.Value as string;
                if (string.IsNullOrWhiteSpace(svalue))
                {
                    MediaFileSystem.UploadAutoFillProperties.Reset(content, autoFillConfig);
                    continue;
                }

                var jo = GetJObject(svalue, false);
                string src;
                if (jo == null)
                {
                    // so we have a non-empty string value that cannot be parsed into a json object
                    // see http://issues.umbraco.org/issue/U4-4756
                    // it can happen when an image is uploaded via the folder browser, in which case
                    // the property value will be the file source eg '/media/23454/hello.jpg' and we
                    // are fixing that anomaly here - does not make any sense at all but... bah...
                    var config = ApplicationContext.Current.Services.DataTypeService
                        .GetPreValuesByDataTypeId(property.PropertyType.DataTypeDefinitionId).FirstOrDefault();
                    var crops = string.IsNullOrWhiteSpace(config) ? "[]" : config;
                    src = svalue;
                    property.Value = "{\"src\": \"" + svalue + "\", \"crops\": " + crops + "}";
                }
                else
                {
                    src = jo["src"] == null ? null : jo["src"].Value<string>();
                }

                if (src == null)
                    MediaFileSystem.UploadAutoFillProperties.Reset(content, autoFillConfig);
                else
                    MediaFileSystem.UploadAutoFillProperties.Populate(content, autoFillConfig, MediaFileSystem.GetRelativePath(src));
            }
        }

        internal class ImageCropperPreValueEditor : PreValueEditor
        {
            [PreValueField("crops", "Define crops", "views/propertyeditors/imagecropper/imagecropper.prevalues.html", Description = "Give the crop an alias and it's default width and height")]
            public string Crops { get; set; }
        }

        #region Application event handler, used to bind to events on startup

        // The ImageCropperPropertyEditor properties own files and as such must manage these files,
        // so we are binding to events in order to make sure that
        // - files are deleted when the owning content/media is
        // - files are copied when the owning content is (NOTE: not supporting media copy here!)
        // - populate the auto-fill properties when files are changing
        // - populate the auto-fill properties when the owning content/media is saved
        //
        // NOTE:
        //  uploading multiple files is NOT a feature of the ImageCropperPropertyEditor
        //
        //  auto-fill properties are recalculated EVERYTIME the content/media is saved,
        //  even if the property has NOT been modified (it could be the same filename but
        //  a different file) - this is accepted (auto-fill props should die)
        //
        // TODO in v8:
        //  for some weird backward compatibility reasons,
        //  - media copy is not supported
        //  - auto-fill properties are not supported for content items
        //  - auto-fill runs on MediaService.Created which makes no sense (no properties yet)

        public void OnApplicationInitialized(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            // nothing
        }

        public void OnApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            // nothing
        }

        public void OnApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            // only if the app is configured
            // see ApplicationEventHandler.ShouldExecute
            if (applicationContext.IsConfigured == false || applicationContext.DatabaseContext.IsDatabaseConfigured == false)
                return;

            MediaService.Created += MediaServiceCreated; // see above - makes no sense
            MediaService.Saving += MediaServiceSaving;
            //MediaService.Copied += MediaServiceCopied; // see above - missing

            ContentService.Copied += ContentServiceCopied;
            //ContentService.Saving += ContentServiceSaving; // see above - missing

            MediaService.Deleted += (sender, args) => args.MediaFilesToDelete.AddRange(
                GetFilesToDelete(args.DeletedEntities.SelectMany(x => x.Properties)));

            MediaService.EmptiedRecycleBin += (sender, args) => args.Files.AddRange(
                GetFilesToDelete(args.AllPropertyData.SelectMany(x => x.Value)));

            ContentService.Deleted += (sender, args) => args.MediaFilesToDelete.AddRange(
                GetFilesToDelete(args.DeletedEntities.SelectMany(x => x.Properties)));

            ContentService.EmptiedRecycleBin += (sender, args) => args.Files.AddRange(
                GetFilesToDelete(args.AllPropertyData.SelectMany(x => x.Value)));

            MemberService.Deleted += (sender, args) => args.MediaFilesToDelete.AddRange(
                GetFilesToDelete(args.DeletedEntities.SelectMany(x => x.Properties)));
        }

        #endregion
    }
}
