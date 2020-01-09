using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
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
        private IMediaService _mediaService;
        private IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;
        private IUmbracoContextAccessor _umbracoContextAccessor;
        private ILogger _logger;

        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        public RichTextPropertyEditor(ILogger logger, IMediaService mediaService, IContentTypeBaseServiceProvider contentTypeBaseServiceProvider, IUmbracoContextAccessor umbracoContextAccessor) : base(logger)
        {
            _mediaService = mediaService;
            _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
            _umbracoContextAccessor = umbracoContextAccessor;
            _logger = logger;
        }

        /// <summary>
        /// Create a custom value editor
        /// </summary>
        /// <returns></returns>
        protected override IDataValueEditor CreateValueEditor() => new RichTextPropertyValueEditor(Attribute, _mediaService, _contentTypeBaseServiceProvider, _umbracoContextAccessor, _logger);

        protected override IConfigurationEditor CreateConfigurationEditor() => new RichTextConfigurationEditor();

        public override IPropertyIndexValueFactory PropertyIndexValueFactory => new RichTextPropertyIndexValueFactory();

        /// <summary>
        /// A custom value editor to ensure that macro syntax is parsed when being persisted and formatted correctly for display in the editor
        /// </summary>
        internal class RichTextPropertyValueEditor : DataValueEditor
        {
            private IMediaService _mediaService;
            private IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;
            private IUmbracoContextAccessor _umbracoContextAccessor;
            private ILogger _logger;

            public RichTextPropertyValueEditor(DataEditorAttribute attribute, IMediaService mediaService, IContentTypeBaseServiceProvider contentTypeBaseServiceProvider, IUmbracoContextAccessor umbracoContextAccessor, ILogger logger)
                : base(attribute)
            {
                _mediaService = mediaService;
                _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
                _umbracoContextAccessor = umbracoContextAccessor;
                _logger = logger;
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

                var propertyValueWithMediaResolved = TemplateUtilities.ResolveMediaFromTextString(val.ToString());
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

                var parseAndSavedTempImages = TemplateUtilities.FindAndPersistPastedTempImages(editorValue.Value.ToString(), mediaParentId, userId, _mediaService, _contentTypeBaseServiceProvider, _logger);
                var editorValueWithMediaUrlsRemoved = TemplateUtilities.RemoveMediaUrlsFromTextString(parseAndSavedTempImages);
                var parsed = MacroTagParser.FormatRichTextContentForPersistence(editorValueWithMediaUrlsRemoved);

                return parsed;
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
