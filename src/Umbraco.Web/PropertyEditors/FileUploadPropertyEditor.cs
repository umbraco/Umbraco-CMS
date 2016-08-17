using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.UploadFieldAlias, "File upload", "fileupload", Icon = "icon-download-alt", Group = "media")]
    public class FileUploadPropertyEditor : PropertyEditor, IApplicationEventHandler
    {
        private readonly MediaFileSystem _mediaFileSystem;
        private readonly IContentSection _contentSettings;
        private readonly ILocalizedTextService _textService;

        public FileUploadPropertyEditor(ILogger logger, MediaFileSystem mediaFileSystem, IContentSection contentSettings, ILocalizedTextService textService)
            : base(logger)
        {
            if (mediaFileSystem == null) throw new ArgumentNullException("mediaFileSystem");
            if (contentSettings == null) throw new ArgumentNullException("contentSettings");
            if (textService == null) throw new ArgumentNullException("textService");
            _applicationStartup = new FileUploadPropertyEditorApplicationStartup(this);
            _mediaFileSystem = mediaFileSystem;
            _contentSettings = contentSettings;
            _textService = textService;
            
        }

        /// <summary>
        /// Creates our custom value editor
        /// </summary>
        /// <returns></returns>
        protected override PropertyValueEditor CreateValueEditor()
        {
            var baseEditor = base.CreateValueEditor();            
            baseEditor.Validators.Add(new UploadFileTypeValidator());
            return new FileUploadPropertyValueEditor(baseEditor, _mediaFileSystem, _contentSettings);
        }

        protected override PreValueEditor CreatePreValueEditor()
        {
            return new FileUploadPreValueEditor(_textService);
        }

        /// <summary>
        /// Ensures any files associated are removed
        /// </summary>
        /// <param name="allPropertyData"></param>
        IEnumerable<string> ServiceEmptiedRecycleBin(Dictionary<int, IEnumerable<Property>> allPropertyData)
        {
            var list = new List<string>();
            //Get all values for any image croppers found
            foreach (var uploadVal in allPropertyData
                .SelectMany(x => x.Value)
                .Where(x => x.PropertyType.PropertyEditorAlias == Constants.PropertyEditors.UploadFieldAlias)
                .Select(x => x.Value)
                .WhereNotNull())
            {
                if (uploadVal.ToString().IsNullOrWhiteSpace() == false)
                {
                    list.Add(uploadVal.ToString());
                }
            }
            return list;
        }

        /// <summary>
        /// Ensures any files associated are removed
        /// </summary>
        /// <param name="deletedEntities"></param>
        IEnumerable<string> ServiceDeleted(IEnumerable<ContentBase> deletedEntities)
        {
            var list = new List<string>();
            foreach (var property in deletedEntities.SelectMany(deletedEntity => deletedEntity
                .Properties
                .Where(x => x.PropertyType.PropertyEditorAlias == Constants.PropertyEditors.UploadFieldAlias
                            && x.Value != null
                            && string.IsNullOrEmpty(x.Value.ToString()) == false)))
            {
                if (property.Value != null && property.Value.ToString().IsNullOrWhiteSpace() == false)
                {
                    list.Add(property.Value.ToString());
                }
            }
            return list;
        }

        /// <summary>
        /// After the content is copied we need to check if there are files that also need to be copied
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
         void ContentServiceCopied(IContentService sender, Core.Events.CopyEventArgs<IContent> e)
        {
            if (e.Original.Properties.Any(x => x.PropertyType.PropertyEditorAlias == Constants.PropertyEditors.UploadFieldAlias))
            {
                bool isUpdated = false;
                var fs = _mediaFileSystem;

                //Loop through properties to check if the content contains media that should be deleted
                foreach (var property in e.Original.Properties.Where(x => x.PropertyType.PropertyEditorAlias == Constants.PropertyEditors.UploadFieldAlias
                    && x.Value != null                                                   
                    && string.IsNullOrEmpty(x.Value.ToString()) == false))
                {
                    if (fs.FileExists(fs.GetRelativePath(property.Value.ToString())))
                    {
                        var currentPath = fs.GetRelativePath(property.Value.ToString());
                        var propertyId = e.Copy.Properties.First(x => x.Alias == property.Alias).Id;
                        var newPath = fs.GetRelativePath(propertyId, System.IO.Path.GetFileName(currentPath));

                        fs.CopyFile(currentPath, newPath);
                        e.Copy.SetValue(property.Alias, fs.GetUrl(newPath));

                        //Copy thumbnails
                        foreach (var thumbPath in fs.GetThumbnails(currentPath))
                        {
                            var newThumbPath = fs.GetRelativePath(propertyId, System.IO.Path.GetFileName(thumbPath));
                            fs.CopyFile(thumbPath, newThumbPath);
                        }
                        isUpdated = true;
                    }
                }

                if (isUpdated)
                {
                    //need to re-save the copy with the updated path value
                    sender.Save(e.Copy);
                }
            }
        }

        void MediaServiceCreating(IMediaService sender, Core.Events.NewEventArgs<IMedia> e)
        {
            AutoFillProperties(e.Entity);
        }

        void MediaServiceSaving(IMediaService sender, Core.Events.SaveEventArgs<IMedia> e)
        {
            foreach (var m in e.SavedEntities)
            {
                AutoFillProperties(m);
            }
        }

        void AutoFillProperties(IContentBase model)
        {
            foreach (var p in model.Properties.Where(x => x.PropertyType.PropertyEditorAlias == Constants.PropertyEditors.UploadFieldAlias))
            {
                var uploadFieldConfigNode =
                    _contentSettings.ImageAutoFillProperties
                                        .FirstOrDefault(x => x.Alias == p.Alias);

                if (uploadFieldConfigNode != null)
                {
                    model.PopulateFileMetaDataProperties(uploadFieldConfigNode, p.Value == null ? string.Empty : p.Value.ToString());
                }
            }            
        }

        /// <summary>
        /// A custom pre-val editor to ensure that the data is stored how the legacy data was stored in 
        /// </summary>
        internal class FileUploadPreValueEditor : ValueListPreValueEditor
        {
            public FileUploadPreValueEditor(ILocalizedTextService textService)
                : base(textService)
            {
                var field = Fields.First();
                field.Description = "Enter a max width/height for each thumbnail";
                field.Name = "Add thumbnail size";
                //need to have some custom validation happening here
                field.Validators.Add(new ThumbnailListValidator());
            }

            /// <summary>
            /// Format the persisted value to work with our multi-val editor.
            /// </summary>
            /// <param name="defaultPreVals"></param>
            /// <param name="persistedPreVals"></param>
            /// <returns></returns>
            public override IDictionary<string, object> ConvertDbToEditor(IDictionary<string, object> defaultPreVals, PreValueCollection persistedPreVals)
            {
                var result = new List<PreValue>();

                //the pre-values just take up one field with a semi-colon delimiter so we'll just parse
                var dictionary = persistedPreVals.FormatAsDictionary();
                if (dictionary.Any())
                {
                    //there should only be one val
                    var delimited = dictionary.First().Value.Value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    for (var index = 0; index < delimited.Length; index++)
                    {
                        result.Add(new PreValue(index, delimited[index]));
                    }
                }

                //the items list will be a dictionary of it's id -> value we need to use the id for persistence for backwards compatibility
                return new Dictionary<string, object> { { "items", result.ToDictionary(x => x.Id, x => PreValueAsDictionary(x)) } };
            }

            private IDictionary<string, object> PreValueAsDictionary(PreValue preValue)
            {
                return new Dictionary<string, object> { { "value", preValue.Value }, { "sortOrder", preValue.SortOrder } };
            }
            /// <summary>
            /// Take the posted values and convert them to a semi-colon separated list so that its backwards compatible
            /// </summary>
            /// <param name="editorValue"></param>
            /// <param name="currentValue"></param>
            /// <returns></returns>
            public override IDictionary<string, PreValue> ConvertEditorToDb(IDictionary<string, object> editorValue, PreValueCollection currentValue)
            {
                var result = base.ConvertEditorToDb(editorValue, currentValue);

                //this should just be a dictionary of values, we want to re-format this so that it is just one value in the dictionary that is 
                // semi-colon delimited
                var values = result.Select(item => item.Value.Value).ToList();

                result.Clear();
                result.Add("thumbs", new PreValue(string.Join(";", values)));
                return result;
            }

            internal class ThumbnailListValidator : IPropertyValidator
            {
                public IEnumerable<ValidationResult> Validate(object value, PreValueCollection preValues, PropertyEditor editor)
                {
                    var json = value as JArray;
                    if (json == null) yield break;

                    //validate each item which is a json object
                    for (var index = 0; index < json.Count; index++)
                    {
                        var i = json[index];
                        var jItem = i as JObject;
                        if (jItem == null || jItem["value"] == null) continue;

                        //NOTE: we will be removing empty values when persisting so no need to validate
                        var asString = jItem["value"].ToString();
                        if (asString.IsNullOrWhiteSpace()) continue;

                        int parsed;
                        if (int.TryParse(asString, out parsed) == false)
                        {
                            yield return new ValidationResult("The value " + asString + " is not a valid number", new[]
                            {
                                //we'll make the server field the index number of the value so it can be wired up to the view
                                "item_" + index.ToInvariantString()
                            });   
                        }
                    }
                }
            }
        }

        #region Application event handler, used to bind to events on startup

        private readonly FileUploadPropertyEditorApplicationStartup _applicationStartup;

        /// <summary>
        /// we're using a sub -class because this has the logic to prevent it from executing if the application is not configured
        /// </summary>
        private class FileUploadPropertyEditorApplicationStartup : ApplicationEventHandler
        {
            private FileUploadPropertyEditor _fileUploadPropertyEditor;

            public FileUploadPropertyEditorApplicationStartup(FileUploadPropertyEditor fileUploadPropertyEditor)
            {
                this._fileUploadPropertyEditor = fileUploadPropertyEditor;
            }

            /// <summary>
            /// We're going to bind to the MediaService Saving event so that we can populate the umbracoFile size, type, etc... label fields
            /// if we find any attached to the current media item.
            /// </summary>
            protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
            {
                MediaService.Saving += _fileUploadPropertyEditor.MediaServiceSaving;
                MediaService.Created += _fileUploadPropertyEditor.MediaServiceCreating;
                ContentService.Copied += _fileUploadPropertyEditor.ContentServiceCopied;

                MediaService.Deleted += (sender, args) =>
                    args.MediaFilesToDelete.AddRange(_fileUploadPropertyEditor.ServiceDeleted(args.DeletedEntities.Cast<ContentBase>()));
                MediaService.EmptiedRecycleBin += (sender, args) =>
                    args.Files.AddRange(_fileUploadPropertyEditor.ServiceEmptiedRecycleBin(args.AllPropertyData));
                ContentService.Deleted += (sender, args) =>
                    args.MediaFilesToDelete.AddRange(_fileUploadPropertyEditor.ServiceDeleted(args.DeletedEntities.Cast<ContentBase>()));
                ContentService.EmptiedRecycleBin += (sender, args) =>
                    args.Files.AddRange(_fileUploadPropertyEditor.ServiceEmptiedRecycleBin(args.AllPropertyData));
                MemberService.Deleted += (sender, args) =>
                    args.MediaFilesToDelete.AddRange(_fileUploadPropertyEditor.ServiceDeleted(args.DeletedEntities.Cast<ContentBase>()));
            }
        }

        public void OnApplicationInitialized(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            //wrap
            _applicationStartup.OnApplicationInitialized(umbracoApplication, applicationContext);
        }
        public void OnApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            //wrap
            _applicationStartup.OnApplicationStarting(umbracoApplication, applicationContext);
        }
        public void OnApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            //wrap
            _applicationStartup.OnApplicationStarted(umbracoApplication, applicationContext);
        }
        #endregion

    }
}
