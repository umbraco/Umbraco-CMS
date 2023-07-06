using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Macros;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Models;
using Umbraco.Web.Templates;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.TinyMCEAlias, "Rich Text Editor", "rte", ValueType = PropertyEditorValueTypes.Text,  HideLabel = false, Group="Rich Content", Icon="icon-browser-window")]
    public class RichTextPropertyEditor : PropertyEditor
    {
        private readonly ILogger _logger;
        private readonly IMediaService _mediaService;

        public RichTextPropertyEditor(
            ILogger logger,
            IMediaService mediaService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));
        }

        /// <summary>
        /// Create a custom value editor
        /// </summary>
        /// <returns></returns>
        protected override PropertyValueEditor CreateValueEditor()
        {
            return new RichTextPropertyValueEditor(base.CreateValueEditor(), _logger, _mediaService);
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
            private readonly ILogger _logger;
            private readonly IMediaService _mediaService;

            public RichTextPropertyValueEditor(
                PropertyValueEditor wrapped,
                ILogger logger,
                IMediaService mediaService)
                : base(wrapped)
            {
                _logger = logger;
                _mediaService = mediaService;
            }

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

                var pastedImages = new RichTextEditorPastedImages(_logger, _mediaService);
                var imageUrlGenerator = new ImageProcessorImageUrlGenerator();
                var parseAndSaveTempImages = pastedImages.FindAndPersistPastedTempImages(editorValue.Value.ToString(), mediaParentId, userId, imageUrlGenerator);
                var parsed = MacroTagParser.FormatRichTextContentForPersistence(parseAndSaveTempImages);
                return parsed;
            }
        }
    }

    
}
