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
using Umbraco.Examine;
using Umbraco.Web.Macros;
using Umbraco.Web.Templates;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents a rich text property editor.
    /// </summary>
    [DataEditor(
        Constants.PropertyEditors.Aliases.TinyMce,
        "Rich Text Editor",
        "rte",
        ValueType = ValueTypes.Text,
        HideLabel = false,
        Group = Constants.PropertyEditors.Groups.RichContent,
        Icon = "icon-browser-window")]
    public class RichTextPropertyEditor : DataEditor
    {
        private IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly HtmlImageSourceParser _imageSourceParser;
        private readonly HtmlLocalLinkParser _localLinkParser;
        private readonly RichTextEditorPastedImages _pastedImages;
        private readonly IImageUrlGenerator _imageUrlGenerator;
        private readonly IHtmlSanitizer _htmlSanitizer;


        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        [Obsolete("Use the constructor which takes an IHtmlSanitizer")]
        public RichTextPropertyEditor(
            ILogger logger,
            IUmbracoContextAccessor umbracoContextAccessor,
            HtmlImageSourceParser imageSourceParser,
            HtmlLocalLinkParser localLinkParser,
            RichTextEditorPastedImages pastedImages)
            : this(logger, umbracoContextAccessor, imageSourceParser, localLinkParser, pastedImages, Current.ImageUrlGenerator)
        {
        }

        [Obsolete("Use the constructor which takes an IHtmlSanitizer")]
        public RichTextPropertyEditor(
            ILogger logger,
            IUmbracoContextAccessor umbracoContextAccessor,
            HtmlImageSourceParser imageSourceParser,
            HtmlLocalLinkParser localLinkParser,
            RichTextEditorPastedImages pastedImages,
            IImageUrlGenerator imageUrlGenerator)
            : this(logger, umbracoContextAccessor, imageSourceParser, localLinkParser, pastedImages, imageUrlGenerator, Current.Factory.GetInstance<IHtmlSanitizer>())
        {
        }

        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        public RichTextPropertyEditor(
            ILogger logger,
            IUmbracoContextAccessor umbracoContextAccessor,
            HtmlImageSourceParser imageSourceParser,
            HtmlLocalLinkParser localLinkParser,
            RichTextEditorPastedImages pastedImages,
            IImageUrlGenerator imageUrlGenerator,
            IHtmlSanitizer htmlSanitizer)
            : base(logger)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
            _imageSourceParser = imageSourceParser;
            _localLinkParser = localLinkParser;
            _pastedImages = pastedImages;
            _imageUrlGenerator = imageUrlGenerator;
            _htmlSanitizer = htmlSanitizer;
        }

        /// <summary>
        /// Create a custom value editor
        /// </summary>
        /// <returns></returns>
        protected override IDataValueEditor CreateValueEditor() => new RichTextPropertyValueEditor(Attribute, _umbracoContextAccessor, _imageSourceParser, _localLinkParser, _pastedImages, _imageUrlGenerator, _htmlSanitizer);

        protected override IConfigurationEditor CreateConfigurationEditor() => new RichTextConfigurationEditor();

        public override IPropertyIndexValueFactory PropertyIndexValueFactory => new RichTextPropertyIndexValueFactory();

        /// <summary>
        /// A custom value editor to ensure that macro syntax is parsed when being persisted and formatted correctly for display in the editor
        /// </summary>
        internal class RichTextPropertyValueEditor : DataValueEditor, IDataValueReference
        {
            private IUmbracoContextAccessor _umbracoContextAccessor;
            private readonly HtmlImageSourceParser _imageSourceParser;
            private readonly HtmlLocalLinkParser _localLinkParser;
            private readonly RichTextEditorPastedImages _pastedImages;
            private readonly IImageUrlGenerator _imageUrlGenerator;
            private readonly IHtmlSanitizer _htmlSanitizer;

            public RichTextPropertyValueEditor(
                DataEditorAttribute attribute,
                IUmbracoContextAccessor umbracoContextAccessor,
                HtmlImageSourceParser imageSourceParser,
                HtmlLocalLinkParser localLinkParser,
                RichTextEditorPastedImages pastedImages,
                IImageUrlGenerator imageUrlGenerator,
                IHtmlSanitizer htmlSanitizer)
                : base(attribute)
            {
                _umbracoContextAccessor = umbracoContextAccessor;
                _imageSourceParser = imageSourceParser;
                _localLinkParser = localLinkParser;
                _pastedImages = pastedImages;
                _imageUrlGenerator = imageUrlGenerator;
                _htmlSanitizer = htmlSanitizer;
            }

            /// <inheritdoc />
            public override object Configuration
            {
                get => base.Configuration;
                set
                {
                    if (value == null)
                        throw new ArgumentNullException(nameof(value));
                    if (!(value is RichTextConfiguration configuration))
                        throw new ArgumentException($"Expected a {typeof(RichTextConfiguration).Name} instance, but got {value.GetType().Name}.", nameof(value));
                    base.Configuration = value;

                    HideLabel = configuration.HideLabel;
                }
            }

            /// <summary>
            /// Format the data for the editor
            /// </summary>
            /// <param name="property"></param>
            /// <param name="dataTypeService"></param>
            /// <param name="culture"></param>
            /// <param name="segment"></param>
            public override object ToEditor(Property property, IDataTypeService dataTypeService, string culture = null, string segment = null)
            {
                var val = property.GetValue(culture, segment);
                if (val == null)
                    return null;

                var propertyValueWithMediaResolved = _imageSourceParser.EnsureImageSources(val.ToString());
                var parsed = MacroTagParser.FormatRichTextPersistedDataForEditor(propertyValueWithMediaResolved, new Dictionary<string, string>());
                return parsed;
            }

            /// <summary>
            /// Format the data for persistence
            /// </summary>
            /// <param name="editorValue"></param>
            /// <param name="currentValue"></param>
            /// <returns></returns>
            public override object FromEditor(Core.Models.Editors.ContentPropertyData editorValue, object currentValue)
            {
                if (editorValue.Value == null)
                    return null;

                var userId = _umbracoContextAccessor.UmbracoContext?.Security?.CurrentUser?.Id ?? Constants.Security.SuperUserId;

                var config = editorValue.DataTypeConfiguration as RichTextConfiguration;
                var mediaParent = config?.MediaParentId;
                var mediaParentId = mediaParent == null ? Guid.Empty : mediaParent.Guid;

                var parseAndSavedTempImages = _pastedImages.FindAndPersistPastedTempImages(editorValue.Value.ToString(), mediaParentId, userId, _imageUrlGenerator);
                var editorValueWithMediaUrlsRemoved = _imageSourceParser.RemoveImageSources(parseAndSavedTempImages);
                var parsed = MacroTagParser.FormatRichTextContentForPersistence(editorValueWithMediaUrlsRemoved);
                var sanitized = _htmlSanitizer.Sanitize(parsed);

                return sanitized.NullOrWhiteSpaceAsNull();
            }

            /// <summary>
            /// Resolve references from <see cref="IDataValueEditor"/> values
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public IEnumerable<UmbracoEntityReference> GetReferences(object value)
            {
                var asString = value == null ? string.Empty : value is string str ? str : value.ToString();

                foreach (var udi in _imageSourceParser.FindUdisFromDataAttributes(asString))
                    yield return new UmbracoEntityReference(udi);

                foreach (var udi in _localLinkParser.FindUdisFromLocalLinks(asString))
                    yield return new UmbracoEntityReference(udi);

                //TODO: Detect Macros too ... but we can save that for a later date, right now need to do media refs
            }
        }

        internal class RichTextPropertyIndexValueFactory : IPropertyIndexValueFactory
        {
            public IEnumerable<KeyValuePair<string, IEnumerable<object>>> GetIndexValues(Property property, string culture, string segment, bool published)
            {
                var val = property.GetValue(culture, segment, published);

                if (!(val is string strVal)) yield break;

                //index the stripped HTML values
                yield return new KeyValuePair<string, IEnumerable<object>>(property.Alias, new object[] { strVal.StripHtml() });
                //store the raw value
                yield return new KeyValuePair<string, IEnumerable<object>>($"{UmbracoExamineIndex.RawFieldPrefix}{property.Alias}", new object[] { strVal });
            }
        }
    }


}
