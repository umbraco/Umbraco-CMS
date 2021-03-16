// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Services.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors
{
    [DataEditor(
        Constants.PropertyEditors.Aliases.UploadField,
        "File upload",
        "fileupload",
        Group = Constants.PropertyEditors.Groups.Media,
        Icon = "icon-download-alt")]
    public class FileUploadPropertyEditor : DataEditor, IMediaUrlGenerator,
        INotificationHandler<ContentCopiedNotification>, INotificationHandler<ContentDeletedNotification>,
        INotificationHandler<MediaDeletedNotification>, INotificationHandler<MediaSavingNotification>
    {
        private readonly IMediaFileSystem _mediaFileSystem;
        private readonly ContentSettings _contentSettings;
        private readonly UploadAutoFillProperties _uploadAutoFillProperties;
        private readonly IDataTypeService _dataTypeService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedTextService _localizedTextService;
        private readonly IContentService _contentService;

        public FileUploadPropertyEditor(
            ILoggerFactory loggerFactory,
            IMediaFileSystem mediaFileSystem,
            IOptions<ContentSettings> contentSettings,
            IDataTypeService dataTypeService,
            ILocalizationService localizationService,
            ILocalizedTextService localizedTextService,
            IShortStringHelper shortStringHelper,
            UploadAutoFillProperties uploadAutoFillProperties,
            IJsonSerializer jsonSerializer,
            IContentService contentService)
            : base(loggerFactory, dataTypeService, localizationService, localizedTextService, shortStringHelper, jsonSerializer)
        {
            _mediaFileSystem = mediaFileSystem ?? throw new ArgumentNullException(nameof(mediaFileSystem));
            _contentSettings = contentSettings.Value;
            _dataTypeService = dataTypeService;
            _localizationService = localizationService;
            _localizedTextService = localizedTextService;
            _uploadAutoFillProperties = uploadAutoFillProperties;
            _contentService = contentService;
        }

        /// <summary>
        /// Creates the corresponding property value editor.
        /// </summary>
        /// <returns>The corresponding property value editor.</returns>
        protected override IDataValueEditor CreateValueEditor()
        {
            var editor = new FileUploadPropertyValueEditor(Attribute, _mediaFileSystem, _dataTypeService, _localizationService, _localizedTextService, ShortStringHelper, Options.Create(_contentSettings), JsonSerializer);
            editor.Validators.Add(new UploadFileTypeValidator(_localizedTextService, Options.Create(_contentSettings)));
            return editor;
        }

        public bool TryGetMediaPath(string alias, object value, out string mediaPath)
        {
            if (alias == Alias)
            {
                mediaPath = value?.ToString();
                return true;
            }
            mediaPath = null;
            return false;
        }

        /// <summary>
        /// Gets a value indicating whether a property is an upload field.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>A value indicating whether a property is an upload field, and (optionally) has a non-empty value.</returns>
        private static bool IsUploadField(IProperty property)
        {
            return property.PropertyType.PropertyEditorAlias == Constants.PropertyEditors.Aliases.UploadField;
        }

        /// <summary>
        /// The paths to all file upload property files contained within a collection of content entities
        /// </summary>
        /// <param name="entities"></param>
        /// <remarks>
        /// This method must be made private once MemberService events have been replaced by notifications
        /// </remarks>
        internal IEnumerable<string> ContainedFilePaths(IEnumerable<IContentBase> entities) => entities
            .SelectMany(x => x.Properties)
            .Where(IsUploadField)
            .SelectMany(GetFilePathsFromPropertyValues)
            .Distinct();

        /// <summary>
        /// Look through all property values stored against the property and resolve any file paths stored
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        private IEnumerable<string> GetFilePathsFromPropertyValues(IProperty prop)
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

        public void Handle(ContentCopiedNotification notification)
        {
            // get the upload field properties with a value
            var properties = notification.Original.Properties.Where(IsUploadField);

            // copy files
            var isUpdated = false;
            foreach (var property in properties)
            {
                //copy each of the property values (variants, segments) to the destination
                foreach (var propertyValue in property.Values)
                {
                    var propVal = property.GetValue(propertyValue.Culture, propertyValue.Segment);
                    if (propVal == null || !(propVal is string str) || str.IsNullOrWhiteSpace())
                    {
                        continue;
                    }

                    var sourcePath = _mediaFileSystem.GetRelativePath(str);
                    var copyPath = _mediaFileSystem.CopyFile(notification.Copy, property.PropertyType, sourcePath);
                    notification.Copy.SetValue(property.Alias, _mediaFileSystem.GetUrl(copyPath), propertyValue.Culture, propertyValue.Segment);
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

        private void DeleteContainedFiles(IEnumerable<IContentBase> deletedEntities)
        {
            var filePathsToDelete = ContainedFilePaths(deletedEntities);
            _mediaFileSystem.DeleteMediaFiles(filePathsToDelete);
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
            var properties = model.Properties.Where(IsUploadField);

            foreach (var property in properties)
            {
                var autoFillConfig = _contentSettings.GetConfig(property.Alias);
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
