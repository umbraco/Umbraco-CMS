using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.ImageCropperAlias, "Image Cropper", "imagecropper", ValueType = "JSON", HideLabel = false)]
    public class ImageCropperPropertyEditor : PropertyEditor
    {
        static ImageCropperPropertyEditor()
        {
            MediaService.Saving += MediaServiceSaving;
            MediaService.Creating += MediaServiceCreating;
        }

        /// <summary>
        /// Creates our custom value editor
        /// </summary>
        /// <returns></returns>
        protected override PropertyValueEditor CreateValueEditor()
        {
            var baseEditor = base.CreateValueEditor();
            return new ImageCropperPropertyValueEditor(baseEditor);
        }

        protected override PreValueEditor CreatePreValueEditor()
        {
            return new ImageCropperPreValueEditor();
        }


        public ImageCropperPropertyEditor()
        {
            _internalPreValues = new Dictionary<string, object>
                {
                    {"crops", "[]"},
                    {"focalPoint", "{left: 0.5, top: 0.5}"},
                    {"src", ""}
                };
        }

        static void MediaServiceCreating(IMediaService sender, Core.Events.NewEventArgs<IMedia> e)
        {
            AutoFillProperties(e.Entity);
        }

        static void MediaServiceSaving(IMediaService sender, Core.Events.SaveEventArgs<IMedia> e)
        {
            foreach (var m in e.SavedEntities)
            {
                AutoFillProperties(m);
            }
        }

        static void AutoFillProperties(IContentBase model)
        {
            var mediaFileSystem = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
            foreach (var p in model.Properties.Where(x => x.PropertyType.Alias == Constants.PropertyEditors.ImageCropperAlias))
            {
                var uploadFieldConfigNode =
                    UmbracoConfig.For.UmbracoSettings().Content.ImageAutoFillProperties
                                        .FirstOrDefault(x => x.Alias == p.Alias);

                if (uploadFieldConfigNode != null)
                {
                    if (p.Value != null){
                         var json = p.Value as JObject;
                         if (json != null && json["src"] != null)
                             model.PopulateFileMetaDataProperties(uploadFieldConfigNode, json["src"].Value<string>());
                         else if (p.Value is string)
                         {
                             var config = ApplicationContext.Current.Services.DataTypeService.GetPreValuesByDataTypeId(p.PropertyType.DataTypeDefinitionId).FirstOrDefault();
                             var crops = !string.IsNullOrEmpty(config) ? config : "[]";
                             p.Value = "{src: '" + p.Value + "', crops: " + crops + "}";
                             model.PopulateFileMetaDataProperties(uploadFieldConfigNode, p.Value);
                         }
                    }else
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
            [PreValueField("crops", "Crop sizes", "cropsizes")]
            public string Crops { get; set; }
        }
    }
}
