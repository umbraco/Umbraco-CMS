using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Templates;

namespace Umbraco.Web.PropertyEditors
{

    /// <summary>
    /// Represents a grid property and parameter editor.
    /// </summary>
    [DataEditor(
        Constants.PropertyEditors.Aliases.Grid,
        "Grid layout",
        "grid",
        HideLabel = true,
        ValueType = ValueTypes.Json,
        Icon = "icon-layout",
        Group = Constants.PropertyEditors.Groups.RichContent)]
    public class GridPropertyEditor : DataEditor
    {
        private IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly HtmlImageSourceParser _imageSourceParser;
        private readonly RichTextEditorPastedImages _pastedImages;
        private readonly HtmlLocalLinkParser _localLinkParser;
        private readonly IImageUrlGenerator _imageUrlGenerator;
        private readonly IHtmlSanitizer _htmlSanitizer;

        [Obsolete("Use the constructor which takes an IHtmlSanitizer")]
        public GridPropertyEditor(ILogger logger,
            IUmbracoContextAccessor umbracoContextAccessor,
            HtmlImageSourceParser imageSourceParser,
            RichTextEditorPastedImages pastedImages,
            HtmlLocalLinkParser localLinkParser)
            : this(logger, umbracoContextAccessor, imageSourceParser, pastedImages, localLinkParser, Current.ImageUrlGenerator)
        {
        }

        [Obsolete("Use the constructor which takes an IHtmlSanitizer")]
        public GridPropertyEditor(ILogger logger,
            IUmbracoContextAccessor umbracoContextAccessor,
            HtmlImageSourceParser imageSourceParser,
            RichTextEditorPastedImages pastedImages,
            HtmlLocalLinkParser localLinkParser,
            IImageUrlGenerator imageUrlGenerator)
            : this(logger, umbracoContextAccessor, imageSourceParser, pastedImages, localLinkParser, imageUrlGenerator, Current.Factory.GetInstance<IHtmlSanitizer>())
        {
        }

        public GridPropertyEditor(ILogger logger,
            IUmbracoContextAccessor umbracoContextAccessor,
            HtmlImageSourceParser imageSourceParser,
            RichTextEditorPastedImages pastedImages,
            HtmlLocalLinkParser localLinkParser,
            IImageUrlGenerator imageUrlGenerator,
            IHtmlSanitizer htmlSanitizer)
            : base(logger)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
            _imageSourceParser = imageSourceParser;
            _pastedImages = pastedImages;
            _localLinkParser = localLinkParser;
            _imageUrlGenerator = imageUrlGenerator;
            _htmlSanitizer = htmlSanitizer;
        }

        public override IPropertyIndexValueFactory PropertyIndexValueFactory => new GridPropertyIndexValueFactory();

        /// <summary>
        /// Overridden to ensure that the value is validated
        /// </summary>
        /// <returns></returns>
        protected override IDataValueEditor CreateValueEditor() => new GridPropertyValueEditor(Attribute, _umbracoContextAccessor, _imageSourceParser, _pastedImages, _localLinkParser, _imageUrlGenerator, _htmlSanitizer);

        protected override IConfigurationEditor CreateConfigurationEditor() => new GridConfigurationEditor();

        internal class GridPropertyValueEditor : DataValueEditor, IDataValueReference
        {
            private readonly IUmbracoContextAccessor _umbracoContextAccessor;
            private readonly HtmlImageSourceParser _imageSourceParser;
            private readonly RichTextEditorPastedImages _pastedImages;
            private readonly RichTextPropertyEditor.RichTextPropertyValueEditor _richTextPropertyValueEditor;
            private readonly MediaPickerPropertyEditor.MediaPickerPropertyValueEditor _mediaPickerPropertyValueEditor;
            private readonly IImageUrlGenerator _imageUrlGenerator;

            [Obsolete("Use the constructor which takes an IHtmlSanitizer")]
            public GridPropertyValueEditor(DataEditorAttribute attribute,
                IUmbracoContextAccessor umbracoContextAccessor,
                HtmlImageSourceParser imageSourceParser,
                RichTextEditorPastedImages pastedImages,
                HtmlLocalLinkParser localLinkParser)
                : this(attribute, umbracoContextAccessor, imageSourceParser, pastedImages, localLinkParser, Current.ImageUrlGenerator)
            {
            }

            [Obsolete("Use the constructor which takes an IHtmlSanitizer")]
            public GridPropertyValueEditor(DataEditorAttribute attribute,
                IUmbracoContextAccessor umbracoContextAccessor,
                HtmlImageSourceParser imageSourceParser,
                RichTextEditorPastedImages pastedImages,
                HtmlLocalLinkParser localLinkParser,
                IImageUrlGenerator imageUrlGenerator)
                : this(attribute, umbracoContextAccessor, imageSourceParser, pastedImages, localLinkParser, imageUrlGenerator, Current.Factory.GetInstance<IHtmlSanitizer>())
            {
            }

            public GridPropertyValueEditor(DataEditorAttribute attribute,
                IUmbracoContextAccessor umbracoContextAccessor,
                HtmlImageSourceParser imageSourceParser,
                RichTextEditorPastedImages pastedImages,
                HtmlLocalLinkParser localLinkParser,
                IImageUrlGenerator imageUrlGenerator,
                IHtmlSanitizer htmlSanitizer)
                : base(attribute)
            {
                _umbracoContextAccessor = umbracoContextAccessor;
                _imageSourceParser = imageSourceParser;
                _pastedImages = pastedImages;
                _imageUrlGenerator = imageUrlGenerator;
                _richTextPropertyValueEditor = new RichTextPropertyEditor.RichTextPropertyValueEditor(attribute, umbracoContextAccessor, imageSourceParser, localLinkParser, pastedImages, _imageUrlGenerator, htmlSanitizer);
                _mediaPickerPropertyValueEditor = new MediaPickerPropertyEditor.MediaPickerPropertyValueEditor(attribute);
            }

            /// <summary>
            /// Format the data for persistence
            /// This to ensure if a RTE is used in a Grid cell/control that we parse it for tmp stored images
            /// to persist to the media library when we go to persist this to the DB
            /// </summary>
            /// <param name="editorValue"></param>
            /// <param name="currentValue"></param>
            /// <returns></returns>
            public override object FromEditor(ContentPropertyData editorValue, object currentValue)
            {
                if (editorValue.Value == null)
                    return null;

                // editorValue.Value is a JSON string of the grid
                var rawJson = editorValue.Value.ToString();
                if (rawJson.IsNullOrWhiteSpace())
                    return null;

                var config = editorValue.DataTypeConfiguration as GridConfiguration;
                var mediaParent = config?.MediaParentId;
                var mediaParentId = mediaParent == null ? Guid.Empty : mediaParent.Guid;

                var grid = DeserializeGridValue(rawJson, out var rtes, out _);

                var userId = _umbracoContextAccessor.UmbracoContext?.Security?.CurrentUser?.Id ?? Constants.Security.SuperUserId;

                // Process the rte values
                foreach (var rte in rtes)
                {
                    // Parse the HTML
                    var html = rte.Value?.ToString();

                    var parseAndSavedTempImages = _pastedImages.FindAndPersistPastedTempImages(html, mediaParentId, userId, _imageUrlGenerator);
                    var editorValueWithMediaUrlsRemoved = _imageSourceParser.RemoveImageSources(parseAndSavedTempImages);

                    rte.Value = editorValueWithMediaUrlsRemoved;
                }

                // Convert back to raw JSON for persisting
                return JsonConvert.SerializeObject(grid, Formatting.None);
            }

            /// <summary>
            /// Ensures that the rich text editor values are processed within the grid
            /// </summary>
            /// <param name="property"></param>
            /// <param name="dataTypeService"></param>
            /// <param name="culture"></param>
            /// <param name="segment"></param>
            /// <returns></returns>
            public override object ToEditor(Property property, IDataTypeService dataTypeService, string culture = null, string segment = null)
            {
                var val = property.GetValue(culture, segment)?.ToString();
                if (val.IsNullOrWhiteSpace()) return string.Empty;

                var grid = DeserializeGridValue(val, out var rtes, out _);

                //process the rte values
                foreach (var rte in rtes.ToList())
                {
                    var html = rte.Value?.ToString();

                    var propertyValueWithMediaResolved = _imageSourceParser.EnsureImageSources(html);
                    rte.Value = propertyValueWithMediaResolved;
                }

                return grid;
            }

            private GridValue DeserializeGridValue(string rawJson, out IEnumerable<GridValue.GridControl> richTextValues, out IEnumerable<GridValue.GridControl> mediaValues)
            {
                var grid = JsonConvert.DeserializeObject<GridValue>(rawJson);

                // Find all controls that use the RTE editor
                var controls = grid.Sections.SelectMany(x => x.Rows.SelectMany(r => r.Areas).SelectMany(a => a.Controls)).ToArray();
                richTextValues = controls.Where(x => x.Editor.Alias.ToLowerInvariant() == "rte");
                mediaValues = controls.Where(x => x.Editor.Alias.ToLowerInvariant() == "media");

                return grid;
            }

            /// <summary>
            /// Resolve references from <see cref="IDataValueEditor"/> values
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public IEnumerable<UmbracoEntityReference> GetReferences(object value)
            {
                var rawJson = value == null ? string.Empty : value is string str ? str : value.ToString();

                if (rawJson.IsNullOrWhiteSpace())
                    yield break;

                DeserializeGridValue(rawJson, out var richTextEditorValues, out var mediaValues);

                foreach (var umbracoEntityReference in richTextEditorValues.SelectMany(x =>
                    _richTextPropertyValueEditor.GetReferences(x.Value)))
                    yield return umbracoEntityReference;

                foreach (var umbracoEntityReference in mediaValues.Where(x => x.Value.HasValues)
                    .SelectMany(x => _mediaPickerPropertyValueEditor.GetReferences(x.Value["udi"])))
                    yield return umbracoEntityReference;
            }
        }
    }
}
