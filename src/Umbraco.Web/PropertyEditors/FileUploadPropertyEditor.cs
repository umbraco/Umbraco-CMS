using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    [DataEditor(Constants.PropertyEditors.Aliases.UploadField, "File upload", "fileupload", Icon = "icon-download-alt", Group = "media")]
    public class FileUploadPropertyEditor : ConfiguredDataEditor
    {
        private readonly MediaFileSystem _mediaFileSystem;

        public FileUploadPropertyEditor(ILogger logger, MediaFileSystem mediaFileSystem)
            : base(logger)
        {
            _mediaFileSystem = mediaFileSystem ?? throw new ArgumentNullException(nameof(mediaFileSystem));
        }

        /// <summary>
        /// Creates the corresponding property value editor.
        /// </summary>
        /// <returns>The corresponding property value editor.</returns>
        protected override IDataValueEditor CreateValueEditor()
        {
            var editor = new FileUploadPropertyValueEditor(Attribute, _mediaFileSystem);
            editor.Validators.Add(new UploadFileTypeValidator());
            return editor;
        }

        /// <summary>
        /// Gets a value indicating whether a property is an upload field.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="ensureValue">A value indicating whether to check that the property has a non-empty value.</param>
        /// <returns>A value indicating whether a property is an upload field, and (optionaly) has a non-empty value.</returns>
        private static bool IsUploadField(Property property, bool ensureValue)
        {
            if (property.PropertyType.PropertyEditorAlias != Constants.PropertyEditors.Aliases.UploadField)
                return false;
            if (ensureValue == false)
                return true;
            var stringValue = property.GetValue() as string;
            return string.IsNullOrWhiteSpace(stringValue) == false;
        }

        /// <summary>
        /// Ensures any files associated are removed
        /// </summary>
        /// <param name="allPropertyData"></param>
        internal IEnumerable<string> ServiceEmptiedRecycleBin(Dictionary<int, IEnumerable<Property>> allPropertyData)
        {
            return allPropertyData.SelectMany(x => x.Value)
                .Where (x => IsUploadField(x, true))
                .Select(x => _mediaFileSystem.GetRelativePath((string)x.GetValue()))
                .ToList();
        }

        /// <summary>
        /// Ensures any files associated are removed
        /// </summary>
        /// <param name="deletedEntities"></param>
        internal IEnumerable<string> ServiceDeleted(IEnumerable<ContentBase> deletedEntities)
        {
            return deletedEntities.SelectMany(x => x.Properties)
                .Where(x => IsUploadField(x, true))
                .Select(x => _mediaFileSystem.GetRelativePath((string) x.GetValue()))
                .ToList();
        }

        /// <summary>
        /// After a content has been copied, also copy uploaded files.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The event arguments.</param>
        internal void ContentServiceCopied(IContentService sender, Core.Events.CopyEventArgs<IContent> args)
        {
            // get the upload field properties with a value
            var properties = args.Original.Properties.Where(x => IsUploadField(x, true));

            // copy files
            var isUpdated = false;
            foreach (var property in properties)
            {
                var sourcePath = _mediaFileSystem.GetRelativePath((string) property.GetValue());
                var copyPath = _mediaFileSystem.CopyFile(args.Copy, property.PropertyType, sourcePath);
                args.Copy.SetValue(property.Alias, _mediaFileSystem.GetUrl(copyPath));
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
        internal void MediaServiceCreated(IMediaService sender, Core.Events.NewEventArgs<IMedia> args)
        {
            AutoFillProperties(args.Entity);
        }

        /// <summary>
        /// After a media has been saved, auto-fill the properties.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The event arguments.</param>
        internal void MediaServiceSaving(IMediaService sender, Core.Events.SaveEventArgs<IMedia> args)
        {
            foreach (var entity in args.SavedEntities)
                AutoFillProperties(entity);
        }

        /// <summary>
        /// After a content item has been saved, auto-fill the properties.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The event arguments.</param>
        internal void ContentServiceSaving(IContentService sender, Core.Events.SaveEventArgs<IContent> args)
        {
            foreach (var entity in args.SavedEntities)
                AutoFillProperties(entity);
        }

        /// <summary>
        /// Auto-fill properties (or clear).
        /// </summary>
        private void AutoFillProperties(IContentBase model)
        {
            var properties = model.Properties.Where(x => IsUploadField(x, false));

            foreach (var property in properties)
            {
                var autoFillConfig = _mediaFileSystem.UploadAutoFillProperties.GetConfig(property.Alias);
                if (autoFillConfig == null) continue;

                foreach (var pvalue in property.Values)
                {
                    var svalue = property.GetValue(pvalue.LanguageId, pvalue.Segment) as string;
                    if (string.IsNullOrWhiteSpace(svalue))
                        _mediaFileSystem.UploadAutoFillProperties.Reset(model, autoFillConfig, pvalue.LanguageId, pvalue.Segment);
                    else
                        _mediaFileSystem.UploadAutoFillProperties.Populate(model, autoFillConfig, _mediaFileSystem.GetRelativePath(svalue), pvalue.LanguageId, pvalue.Segment);
                }
            }
        }
    }
}
