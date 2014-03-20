using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Newtonsoft.Json.Linq;
using umbraco.cms.businesslogic.Files;
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
            var baseEditor = base.CreateValueEditor();            
            baseEditor.Validators.Add(new UploadFileTypeValidator());
            return new FileUploadPropertyValueEditor(baseEditor);
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
            foreach (var p in model.Properties.Where(x => x.PropertyType.PropertyEditorAlias == Constants.PropertyEditors.UploadFieldAlias))
            {
                var uploadFieldConfigNode =
                    UmbracoConfig.For.UmbracoSettings().Content.ImageAutoFillProperties
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
            public FileUploadPreValueEditor()
                : base()
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

    }
}
