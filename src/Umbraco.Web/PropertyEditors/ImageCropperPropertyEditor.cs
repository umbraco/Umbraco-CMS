using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.ImageCropperAlias, "Image Cropper", "imagecropper", ValueType = PropertyEditorValueTypes.Json, HideLabel = false, Group="media", Icon="icon-crop")]
    public class ImageCropperPropertyEditor : PropertyEditor
    {
        private readonly MediaFileSystem _mediaFileSystem;
        private readonly IContentSection _contentSettings;

        public ImageCropperPropertyEditor(ILogger logger, MediaFileSystem mediaFileSystem, IContentSection contentSettings)
            : base(logger)
        {
            if (mediaFileSystem == null) throw new ArgumentNullException(nameof(mediaFileSystem));
            if (contentSettings == null) throw new ArgumentNullException(nameof(contentSettings));

            _mediaFileSystem = mediaFileSystem;
            _contentSettings = contentSettings;

            _internalPreValues = new Dictionary<string, object>
                {
                    {"focalPoint", "{left: 0.5, top: 0.5}"},
                    {"src", ""}
                };            
        }

        /// <summary>
        /// Creates our custom value editor
        /// </summary>
        /// <returns></returns>
        protected override PropertyValueEditor CreateValueEditor()
        {
            var baseEditor = base.CreateValueEditor();
            return new ImageCropperPropertyValueEditor(baseEditor, Logger, _mediaFileSystem, _contentSettings);
        }

        protected override PreValueEditor CreatePreValueEditor()
        {
            return new ImageCropperPreValueEditor();
        }


        /// <summary>
        /// Ensures any files associated are removed
        /// </summary>
        /// <param name="allPropertyData"></param>
        internal IEnumerable<string> ServiceEmptiedRecycleBin(Dictionary<int, IEnumerable<Property>> allPropertyData)
        {
            var list = new List<string>();
            //Get all values for any image croppers found
            foreach (var cropperVal in allPropertyData
                .SelectMany(x => x.Value)
                .Where(x => x.PropertyType.PropertyEditorAlias == Constants.PropertyEditors.ImageCropperAlias)  
                .Select(x => x.Value)
                .WhereNotNull())
            {
                JObject json;
                try
                {
                    json = JsonConvert.DeserializeObject<JObject>(cropperVal.ToString());
                }
                catch (Exception ex)
                {
                    Logger.Error<ImageCropperPropertyEditor>("An error occurred parsing the value stored in the image cropper value: " + cropperVal, ex);
                    continue;
                }

                if (json["src"] != null && json["src"].ToString().IsNullOrWhiteSpace() == false)
                {
                    list.Add(json["src"].ToString());
                }
            }
            return list;
        }

        /// <summary>
        /// Ensures any files associated are removed
        /// </summary>
        /// <param name="deletedEntities"></param>
        internal IEnumerable<string> ServiceDeleted(IEnumerable<ContentBase> deletedEntities)
        {
            var list = new List<string>();
            foreach (var property in deletedEntities.SelectMany(deletedEntity => deletedEntity
                .Properties
                .Where(x => x.PropertyType.PropertyEditorAlias == Constants.PropertyEditors.ImageCropperAlias
                            && x.Value != null
                            && string.IsNullOrEmpty(x.Value.ToString()) == false)))
            {
                JObject json;
                try
                {
                    json = JsonConvert.DeserializeObject<JObject>(property.Value.ToString());
                }
                catch (Exception ex)
                {
                    Logger.Error<ImageCropperPropertyEditor>("An error occurred parsing the value stored in the image cropper value: " + property.Value, ex);
                    continue;
                }

                if (json["src"] != null && json["src"].ToString().IsNullOrWhiteSpace() == false)
                {
                    list.Add(json["src"].ToString());
                }
            }
            return list;
        }

        /// <summary>
        /// After the content is copied we need to check if there are files that also need to be copied
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void ContentServiceCopied(IContentService sender, Core.Events.CopyEventArgs<IContent> e)
        {
            if (e.Original.Properties.Any(x => x.PropertyType.PropertyEditorAlias == Constants.PropertyEditors.ImageCropperAlias))
            {
                bool isUpdated = false;
                var fs = _mediaFileSystem;

                //Loop through properties to check if the content contains media that should be deleted
                foreach (var property in e.Original.Properties.Where(x => x.PropertyType.PropertyEditorAlias == Constants.PropertyEditors.ImageCropperAlias
                    && x.Value != null                                                   
                    && string.IsNullOrEmpty(x.Value.ToString()) == false))
                {
                    JObject json;
                    try
                    {
                        json = JsonConvert.DeserializeObject<JObject>(property.Value.ToString());
                    }
                    catch (Exception ex)
                    {
                        Logger.Error<ImageCropperPropertyEditor>("An error occurred parsing the value stored in the image cropper value: " + property.Value, ex);
                        continue;
                    }

                    if (json["src"] != null && json["src"].ToString().IsNullOrWhiteSpace() == false)
                    {
                        if (fs.FileExists(fs.GetRelativePath(json["src"].ToString())))
                        {
                            var currentPath = fs.GetRelativePath(json["src"].ToString());
                            var propertyId = e.Copy.Properties.First(x => x.Alias == property.Alias).Id;
                            var newPath = fs.GetRelativePath(propertyId, System.IO.Path.GetFileName(currentPath));

                            fs.CopyFile(currentPath, newPath);
                            json["src"] = fs.GetUrl(newPath);
                            e.Copy.SetValue(property.Alias, json.ToString());

                            //Copy thumbnails
                            foreach (var thumbPath in fs.GetThumbnails(currentPath))
                            {
                                var newThumbPath = fs.GetRelativePath(propertyId, System.IO.Path.GetFileName(thumbPath));
                                fs.CopyFile(thumbPath, newThumbPath);
                            }
                            isUpdated = true;
                        }
                    }
                }

                if (isUpdated)
                {
                    //need to re-save the copy with the updated path value
                    sender.Save(e.Copy);
                }
            }
        }

        internal void MediaServiceCreated(IMediaService sender, Core.Events.NewEventArgs<IMedia> e)
        {
            AutoFillProperties(e.Entity);
        }

        internal void MediaServiceSaving(IMediaService sender, Core.Events.SaveEventArgs<IMedia> e)
        {
            foreach (var m in e.SavedEntities)
            {
                AutoFillProperties(m);
            }
        }

        void AutoFillProperties(IContentBase model)
        {
            foreach (var p in model.Properties.Where(x => x.PropertyType.PropertyEditorAlias == Constants.PropertyEditors.ImageCropperAlias))
            {
                var uploadFieldConfigNode =
                    _contentSettings.ImageAutoFillProperties
                                        .FirstOrDefault(x => x.Alias == p.Alias);

                if (uploadFieldConfigNode != null)
                {
                    if (p.Value != null)
                    {                        
                        JObject json = null;
                        try
                        {
                            json = JObject.Parse((string)p.Value);
                        }
                        catch (JsonException)
                        {
                            //note: we are swallowing this exception because in some cases a normal string/non json value will be passed in which will just be the 
                            // file path like /media/23454/hello.jpg
                            // This will happen everytime an image is uploaded via the folder browser and we don't really want to pollute the log since it's not actually
                            // a problem and we take care of this below.
                            // see: http://issues.umbraco.org/issue/U4-4756
                        }
                        if (json != null && json["src"] != null)
                        {
                            model.PopulateFileMetaDataProperties(uploadFieldConfigNode, json["src"].Value<string>());
                        }
                        else if (p.Value is string)
                        {
                            var src = p.Value == null ? string.Empty : p.Value.ToString();
                            var config = Current.Services.DataTypeService.GetPreValuesByDataTypeId(p.PropertyType.DataTypeDefinitionId).FirstOrDefault();
                            var crops = string.IsNullOrEmpty(config) == false ? config : "[]";
                            p.Value = "{src: '" + p.Value + "', crops: " + crops + "}";
                            //Only provide the source path, not the whole JSON value
                            model.PopulateFileMetaDataProperties(uploadFieldConfigNode, src);
                        }
                    }
                    else
                        model.ResetFileMetaDataProperties(uploadFieldConfigNode);
                }
            }
        }

        private IDictionary<string, object> _internalPreValues;
        public override IDictionary<string, object> DefaultPreValues
        {
            get { return _internalPreValues; }
            set { _internalPreValues = value; }
        }

        internal class ImageCropperPreValueEditor : PreValueEditor
        {
            [PreValueField("crops", "Crop sizes", "views/propertyeditors/imagecropper/imagecropper.prevalues.html")]
            public string Crops { get; set; }
        }
    }
}
