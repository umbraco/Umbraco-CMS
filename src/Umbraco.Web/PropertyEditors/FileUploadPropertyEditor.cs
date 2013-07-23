using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.UploadField, "File upload", "fileupload")]
    public class FileUploadPropertyEditor : PropertyEditor
    {
        /// <summary>
        /// We're going to bind to the MediaService Saving event so that we can populate the umbracoFile size, type, etc... label fields
        /// if we find any attached to the current media item.
        /// </summary>
        /// <remarks>
        /// I think this kind of logic belongs on this property editor, I guess it could exist elsewhere but it all has to do with the upload field.
        /// </remarks>
        static FileUploadPropertyEditor()
        {
            MediaService.Saving += MediaServiceSaving;
            MediaService.Creating += MediaServiceCreating;
        }

        /// <summary>
        /// Creates our custom value editor
        /// </summary>
        /// <returns></returns>
        protected override ValueEditor CreateValueEditor()
        {
            //TODO: Ensure we assign custom validation for uploaded file types!
            
            var baseEditor = base.CreateValueEditor();

            return new FileUploadValueEditor
                {
                    View = baseEditor.View
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
            if (UmbracoSettings.ImageAutoFillImageProperties != null)
            {
                foreach (var p in model.Properties)
                {
                    var uploadFieldConfigNode =
                        UmbracoSettings.ImageAutoFillImageProperties.SelectSingleNode(
                            string.Format("uploadField [@alias = \"{0}\"]", p.Alias));

                    if (uploadFieldConfigNode != null && p.Value != null && p.Value is string && ((string)p.Value).IsNullOrWhiteSpace() == false)
                    {
                        var umbracoFile = new UmbracoMediaFile(IOHelper.MapPath((string)p.Value));
                        FillProperties(uploadFieldConfigNode, model, umbracoFile);
                    }
                }
            }
        }

        private static void FillProperties(XmlNode uploadFieldConfigNode, IContentBase content, UmbracoMediaFile um)
        {
            // only add dimensions to web images
            UpdateContentProperty(uploadFieldConfigNode, content, "widthFieldAlias", um.SupportsResizing ? um.GetDimensions().Width.ToString() : string.Empty);
            UpdateContentProperty(uploadFieldConfigNode, content, "heightFieldAlias", um.SupportsResizing ? um.GetDimensions().Height.ToString() : string.Empty);

            UpdateContentProperty(uploadFieldConfigNode, content, "lengthFieldAlias", um.Length);
            UpdateContentProperty(uploadFieldConfigNode, content, "extensionFieldAlias", um.Extension);
        }

        private static void UpdateContentProperty(XmlNode uploadFieldConfigNode, IContentBase content, string configPropertyAlias, object propertyValue)
        {
            var propertyNode = uploadFieldConfigNode.SelectSingleNode(configPropertyAlias);
            if (propertyNode != null && string.IsNullOrEmpty(propertyNode.FirstChild.Value) == false)
            {
                var property = content.Properties[propertyNode.FirstChild.Value];
                if (property != null)
                {
                    property.Value = propertyValue;
                }
            }
        }

    }
}