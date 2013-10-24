using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml;
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
    [PropertyEditor(Constants.PropertyEditors.UploadFieldAlias, "File upload", "fileupload")]
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
        protected override PropertyValueEditor CreateValueEditor()
        {
            //TODO: Ensure we assign custom validation for uploaded file types!
            
            var baseEditor = base.CreateValueEditor();

            return new FileUploadPropertyValueEditor
                {
                    View = baseEditor.View
                };
        }

        protected override PreValueEditor CreatePreValueEditor()
        {
            return new FileUploadPreValueEditor();
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
            foreach (var p in model.Properties)
            {
                var uploadFieldConfigNode =
                    UmbracoConfig.For.UmbracoSettings().Content.ImageAutoFillProperties
                                        .FirstOrDefault(x => x.Alias == p.Alias);

                if (uploadFieldConfigNode != null)
                {
                    //now we need to check if there is a value
                    if (p.Value is string && ((string) p.Value).IsNullOrWhiteSpace() == false)
                    {
                        //there might be multiple, we can only process the first one!
                        var split = ((string) p.Value).Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                        if (split.Any())
                        {
                            var umbracoFile = new UmbracoMediaFile(IOHelper.MapPath(split[0]));
                            FillProperties(uploadFieldConfigNode, model, umbracoFile);
                        }
                    }
                    else
                    {
                        //there's no value so need to reset to zero
                        ResetProperties(uploadFieldConfigNode, model);
                    }
                }
            }            
        }

        private static void ResetProperties(IImagingAutoFillUploadField uploadFieldConfigNode, IContentBase content)
        {
            if (content.Properties.Contains(uploadFieldConfigNode.WidthFieldAlias))
                content.Properties[uploadFieldConfigNode.WidthFieldAlias].Value = string.Empty;
            
            if (content.Properties.Contains(uploadFieldConfigNode.HeightFieldAlias))
                content.Properties[uploadFieldConfigNode.HeightFieldAlias].Value = string.Empty;

            if (content.Properties.Contains(uploadFieldConfigNode.LengthFieldAlias))
                content.Properties[uploadFieldConfigNode.LengthFieldAlias].Value = string.Empty;

            if (content.Properties.Contains(uploadFieldConfigNode.ExtensionFieldAlias))
                content.Properties[uploadFieldConfigNode.ExtensionFieldAlias].Value = string.Empty;
        }

        private static void FillProperties(IImagingAutoFillUploadField uploadFieldConfigNode, IContentBase content, UmbracoMediaFile um)
        {
            var size = um.SupportsResizing ? (Size?)um.GetDimensions() : null;

            if (content.Properties.Contains(uploadFieldConfigNode.WidthFieldAlias))
                content.Properties[uploadFieldConfigNode.WidthFieldAlias].Value = size.HasValue ? size.Value.Width.ToInvariantString() : string.Empty;

            if (content.Properties.Contains(uploadFieldConfigNode.HeightFieldAlias))
                content.Properties[uploadFieldConfigNode.HeightFieldAlias].Value = size.HasValue ? size.Value.Height.ToInvariantString() : string.Empty;

            if (content.Properties.Contains(uploadFieldConfigNode.LengthFieldAlias))
                content.Properties[uploadFieldConfigNode.LengthFieldAlias].Value = um.Length;

            if (content.Properties.Contains(uploadFieldConfigNode.ExtensionFieldAlias))
                content.Properties[uploadFieldConfigNode.ExtensionFieldAlias].Value = um.Extension;
        }

        /// <summary>
        /// A custom pre-val editor to ensure that the data is stored how the legacy data was stored in 
        /// </summary>
        internal class FileUploadPreValueEditor : ValueListPreValueEditor
        {
            /// <summary>
            /// Format the persisted value to work with our multi-val editor.
            /// </summary>
            /// <param name="defaultPreVals"></param>
            /// <param name="persistedPreVals"></param>
            /// <returns></returns>
            public override IDictionary<string, object> ConvertDbToEditor(IDictionary<string, object> defaultPreVals, PreValueCollection persistedPreVals)
            {
                var result = new Dictionary<string, object>();

                //the pre-values just take up one field with a semi-colon delimiter so we'll just parse
                var dictionary = persistedPreVals.FormatAsDictionary();
                if (dictionary.Any())
                {
                    //there should only be one val
                    var delimited = dictionary.First().Value.Value.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);
                    for (var index = 0; index < delimited.Length; index++)
                    {
                        result.Add(index.ToInvariantString(), delimited[index]);
                    }
                }

                //the items list will be a dictionary of it's id -> value we need to use the id for persistence for backwards compatibility
                return new Dictionary<string, object> { { "items", result } };
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
                var values = result.Select(item => item.Value).ToList();

                result.Clear();
                result.Add("thumbs", new PreValue(string.Join(";", values)));
                return result;
            }
        }

    }
}