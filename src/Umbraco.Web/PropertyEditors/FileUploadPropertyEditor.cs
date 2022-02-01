using System;
using System.Collections.Generic;
using System.Linq;
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
    [DataEditor(
        Constants.PropertyEditors.Aliases.UploadField,
        "File upload",
        "fileupload",
        Group = Constants.PropertyEditors.Groups.Media,
        Icon = "icon-download-alt")]
    public class FileUploadPropertyEditor : DataEditor, IDataEditorWithMediaPath
    {
        private readonly IMediaFileSystem _mediaFileSystem;
        private readonly IContentSection _contentSection;
        private readonly UploadAutoFillProperties _uploadAutoFillProperties;

        public FileUploadPropertyEditor(ILogger logger, IMediaFileSystem mediaFileSystem, IContentSection contentSection)
            : base(logger)
        {
            _mediaFileSystem = mediaFileSystem ?? throw new ArgumentNullException(nameof(mediaFileSystem));
            _contentSection = contentSection;
            _uploadAutoFillProperties = new UploadAutoFillProperties(_mediaFileSystem, logger, contentSection);
        }


        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new FileUploadConfigurationEditor();

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

        public string GetMediaPath(object value) => value?.ToString();

        /// <summary>
        /// Gets a value indicating whether a property is an upload field.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>A value indicating whether a property is an upload field, and (optionally) has a non-empty value.</returns>
        private static bool IsUploadField(Property property)
        {
            return property.PropertyType.PropertyEditorAlias == Constants.PropertyEditors.Aliases.UploadField;
        }

        /// <summary>
        /// Ensures any files associated are removed
        /// </summary>
        /// <param name="deletedEntities"></param>
        internal IEnumerable<string> ServiceDeleted(IEnumerable<ContentBase> deletedEntities)
        {
            return deletedEntities.SelectMany(x => x.Properties)
                .Where(IsUploadField)
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
            var propVals = prop.Values;
            foreach (var propertyValue in propVals)
            {
                //check if the published value contains data and return it
                var propVal = propertyValue.PublishedValue;
                if (propVal != null && propVal is string str1 && !str1.IsNullOrWhiteSpace())
                    yield return _mediaFileSystem.GetRelativePath(str1);

                //check if the edited value contains data and return it
                propVal = propertyValue.EditedValue;
                if (propVal != null && propVal is string str2 && !str2.IsNullOrWhiteSpace())
                    yield return _mediaFileSystem.GetRelativePath(str2);
            }
        }

        /// <summary>
        /// After a content has been copied, also copy uploaded files.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The event arguments.</param>
        internal void ContentServiceCopied(IContentService sender, Core.Events.CopyEventArgs<IContent> args)
        {
            // get the upload field properties with a value
            var properties = args.Original.Properties.Where(IsUploadField);

            // copy files
            var isUpdated = false;
            foreach (var property in properties)
            {
                //copy each of the property values (variants, segments) to the destination
                foreach (var propertyValue in property.Values)
                {
                    var propVal = property.GetValue(propertyValue.Culture, propertyValue.Segment);
                    if (propVal == null || !(propVal is string str) || str.IsNullOrWhiteSpace()) continue;
                    var sourcePath = _mediaFileSystem.GetRelativePath(str);
                    var copyPath = _mediaFileSystem.CopyFile(args.Copy, property.PropertyType, sourcePath);
                    args.Copy.SetValue(property.Alias, _mediaFileSystem.GetUrl(copyPath), propertyValue.Culture, propertyValue.Segment);
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
            var properties = model.Properties.Where(IsUploadField);

            foreach (var property in properties)
            {
                var autoFillConfig = _contentSection.GetConfig(property.Alias);
                if (autoFillConfig == null) continue;

                foreach (var pvalue in property.Values)
                {
                    var svalue = property.GetValue(pvalue.Culture, pvalue.Segment) as string;
                    if (string.IsNullOrWhiteSpace(svalue))
                        _uploadAutoFillProperties.Reset(model, autoFillConfig, pvalue.Culture, pvalue.Segment);
                    else
                        _uploadAutoFillProperties.Populate(model, autoFillConfig, _mediaFileSystem.GetRelativePath(svalue), pvalue.Culture, pvalue.Segment);
                }
            }
        }
    }
}
