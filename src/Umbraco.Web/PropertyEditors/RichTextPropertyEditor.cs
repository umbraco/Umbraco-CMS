using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Macros;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Templates;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.TinyMCEAlias, "Rich Text Editor", "rte", ValueType = PropertyEditorValueTypes.Text, HideLabel = false, Group = "Rich Content", Icon = "icon-browser-window")]
    public class RichTextPropertyEditor : PropertyEditor
    {

        /// <summary>
        /// Create a custom value editor
        /// </summary>
        /// <returns></returns>
        protected override PropertyValueEditor CreateValueEditor()
        {
            return new RichTextPropertyValueEditor(base.CreateValueEditor());
        }

        protected override PreValueEditor CreatePreValueEditor()
        {
            return new RichTextPreValueEditor();
        }


        /// <summary>
        /// A custom value editor to ensure that macro syntax is parsed when being persisted and formatted correctly for display in the editor
        /// </summary>
        internal class RichTextPropertyValueEditor : PropertyValueEditorWrapper
        {
            public RichTextPropertyValueEditor(
                PropertyValueEditor wrapped)
                : base(wrapped)
            {}

            /// <summary>
            /// override so that we can hide the label based on the pre-value
            /// </summary>
            /// <param name="preValues"></param>
            public override void ConfigureForDisplay(Core.Models.PreValueCollection preValues)
            {
                base.ConfigureForDisplay(preValues);
                var asDictionary = preValues.FormatAsDictionary();
                if (asDictionary.ContainsKey("hideLabel"))
                {
                    var boolAttempt = asDictionary["hideLabel"].Value.TryConvertTo<bool>();
                    if (boolAttempt.Success)
                    {
                        HideLabel = boolAttempt.Result;
                    }
                }
            }

            /// <summary>
            /// Format the data for the editor
            /// </summary>
            /// <param name="property"></param>
            /// <param name="propertyType"></param>
            /// <param name="dataTypeService"></param>
            /// <returns></returns>
            public override object ConvertDbToEditor(Property property, PropertyType propertyType, IDataTypeService dataTypeService)
            {
                if (property.Value == null)
                    return null;

                var propertyValueWithMediaResolved = TemplateUtilities.ResolveMediaFromTextString(property.Value.ToString());
                var parsed = MacroTagParser.FormatRichTextPersistedDataForEditor(propertyValueWithMediaResolved, new Dictionary<string, string>());
                return parsed;
            }

            /// <summary>
            /// Format the data for persistence
            /// </summary>
            /// <param name="editorValue"></param>
            /// <param name="currentValue"></param>
            /// <returns></returns>
            public override object ConvertEditorToDb(Core.Models.Editors.ContentPropertyData editorValue, object currentValue)
            {
                if (editorValue.Value == null)
                    return null;

                var userId = UmbracoContext.Current.Security?.CurrentUser?.Id ?? Constants.Security.SuperUserId;

                // You can configure the media parent folder in V8+, but this is supposed to handle images for V7 once and for all
                // so we will just save the images in the Media root folder
                var mediaParentId = Guid.Empty;
                ILogger logger = UmbracoContext.Current.Application.ProfilingLogger.Logger;
                IMediaService mediaService = UmbracoContext.Current.Application.Services.MediaService;

                var pastedImages = new RichTextEditorPastedImages(logger, mediaService);
                var parseAndSaveDataUriImages = pastedImages.FindAndPersistBase64Images(editorValue.Value.ToString(), mediaParentId, userId);
                var parseAndSaveTempImages = pastedImages.FindAndPersistPastedTempImages(parseAndSaveDataUriImages, mediaParentId, userId);
                var parsed = MacroTagParser.FormatRichTextContentForPersistence(parseAndSaveTempImages);
                return parsed;
            }
        }
    }


}
