using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.UploadFieldAlias, "File upload", "fileupload", Icon = "icon-download-alt", Group = "media")]
    public class FileUploadPropertyEditor : PropertyEditor, IApplicationEventHandler
    {
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
            baseEditor.Validators.Add(new UploadFileTypeValidator());
            return new FileUploadPropertyValueEditor(baseEditor, MediaFileSystem);
        }
        
        /// <summary>
        /// Gets a value indicating whether a property is an upload field.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="ensureValue">A value indicating whether to check that the property has a non-empty value.</param>
        /// <returns>A value indicating whether a property is an upload field, and (optionaly) has a non-empty value.</returns>
        private static bool IsUploadField(Property property, bool ensureValue)
        {
            if (property.PropertyType.PropertyEditorAlias != Constants.PropertyEditors.UploadFieldAlias)
                return false;
            if (ensureValue == false)
                return true;
            return property.Value is string && string.IsNullOrWhiteSpace((string) property.Value) == false;
        }

        /// <summary>
        /// Gets the files that need to be deleted when entities are deleted.
        /// </summary>
        /// <param name="properties">The properties that were deleted.</param>
        static IEnumerable<string> GetFilesToDelete(IEnumerable<Property> properties)
        {
            return properties
                .Where(x => IsUploadField(x, true))
                .Select(x => MediaFileSystem.GetRelativePath((string) x.Value))
                .ToList();
        }

        /// <summary>
        /// After a content has been copied, also copy uploaded files.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The event arguments.</param>
        static void ContentServiceCopied(IContentService sender, Core.Events.CopyEventArgs<IContent> args)
        {
            // get the upload field properties with a value
            var properties = args.Original.Properties.Where(x => IsUploadField(x, true));

            // copy files
            var isUpdated = false;
            foreach (var property in properties)
            {
                var sourcePath = MediaFileSystem.GetRelativePath((string) property.Value);
                var copyPath = MediaFileSystem.CopyFile(args.Copy, property.PropertyType, sourcePath);
                args.Copy.SetValue(property.Alias, MediaFileSystem.GetUrl(copyPath));
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
            var properties = content.Properties.Where(x => IsUploadField(x, false));

            foreach (var property in properties)
            {
                var autoFillConfig = MediaFileSystem.UploadAutoFillProperties.GetConfig(property.Alias);
                if (autoFillConfig == null) continue;

                var svalue = property.Value as string;
                if (string.IsNullOrWhiteSpace(svalue))
                    MediaFileSystem.UploadAutoFillProperties.Reset(content, autoFillConfig);
                else
                    MediaFileSystem.UploadAutoFillProperties.Populate(content, autoFillConfig, MediaFileSystem.GetRelativePath(svalue));
            }
        }        

        #region Application event handler, used to bind to events on startup

        // The FileUploadPropertyEditor properties own files and as such must manage these files,
        // so we are binding to events in order to make sure that
        // - files are deleted when the owning content/media is
        // - files are copied when the owning content is
        // - populate the auto-fill properties when the owning content/media is saved
        //
        // NOTE:
        //  although some code fragments seem to want to support uploading multiple files,
        //  this is NOT a feature of the FileUploadPropertyEditor and is NOT supported
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
