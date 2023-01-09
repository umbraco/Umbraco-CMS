// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Templates;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Infrastructure.Macros;
using Umbraco.Cms.Infrastructure.Templates;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a rich text property editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.TinyMce,
    "Rich Text Editor",
    "rte",
    ValueType = ValueTypes.Text,
    HideLabel = false,
    Group = Constants.PropertyEditors.Groups.RichContent,
    Icon = "icon-browser-window",
    ValueEditorIsReusable = true)]
public class RichTextPropertyEditor : DataEditor
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IEditorConfigurationParser _editorConfigurationParser;
    private readonly HtmlImageSourceParser _imageSourceParser;
    private readonly IImageUrlGenerator _imageUrlGenerator;
    private readonly IIOHelper _ioHelper;
    private readonly HtmlLocalLinkParser _localLinkParser;
    private readonly IHtmlMacroParameterParser _macroParameterParser;
    private readonly RichTextEditorPastedImages _pastedImages;

    [Obsolete("Use the constructor which takes an IHtmlMacroParameterParser instead")]
    public RichTextPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        HtmlImageSourceParser imageSourceParser,
        HtmlLocalLinkParser localLinkParser,
        RichTextEditorPastedImages pastedImages,
        IIOHelper ioHelper,
        IImageUrlGenerator imageUrlGenerator,
        IHtmlMacroParameterParser macroParameterParser)
        : this(
            dataValueEditorFactory,
            backOfficeSecurityAccessor,
            imageSourceParser,
            localLinkParser,
            pastedImages,
            ioHelper,
            imageUrlGenerator,
            macroParameterParser,
            StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
    {
    }

    [Obsolete("Use the constructor which takes an IHtmlMacroParameterParser instead")]
    public RichTextPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        HtmlImageSourceParser imageSourceParser,
        HtmlLocalLinkParser localLinkParser,
        RichTextEditorPastedImages pastedImages,
        IIOHelper ioHelper,
        IImageUrlGenerator imageUrlGenerator)
        : this(
            dataValueEditorFactory,
            backOfficeSecurityAccessor,
            imageSourceParser,
            localLinkParser,
            pastedImages,
            ioHelper,
            imageUrlGenerator,
            StaticServiceProvider.Instance.GetRequiredService<IHtmlMacroParameterParser>(),
            StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
    {
    }

    /// <summary>
    ///     The constructor will setup the property editor based on the attribute if one is found.
    /// </summary>
    public RichTextPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        HtmlImageSourceParser imageSourceParser,
        HtmlLocalLinkParser localLinkParser,
        RichTextEditorPastedImages pastedImages,
        IIOHelper ioHelper,
        IImageUrlGenerator imageUrlGenerator,
        IHtmlMacroParameterParser macroParameterParser,
        IEditorConfigurationParser editorConfigurationParser)
        : base(dataValueEditorFactory)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _imageSourceParser = imageSourceParser;
        _localLinkParser = localLinkParser;
        _pastedImages = pastedImages;
        _ioHelper = ioHelper;
        _imageUrlGenerator = imageUrlGenerator;
        _macroParameterParser = macroParameterParser;
        _editorConfigurationParser = editorConfigurationParser;
        SupportsReadOnly = true;
    }

    public override IPropertyIndexValueFactory PropertyIndexValueFactory => new RichTextPropertyIndexValueFactory();

    /// <summary>
    ///     Create a custom value editor
    /// </summary>
    /// <returns></returns>
    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<RichTextPropertyValueEditor>(Attribute!);

    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new RichTextConfigurationEditor(_ioHelper, _editorConfigurationParser);

    /// <summary>
    ///     A custom value editor to ensure that macro syntax is parsed when being persisted and formatted correctly for
    ///     display in the editor
    /// </summary>
    internal class RichTextPropertyValueEditor : DataValueEditor, IDataValueReference
    {
        private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
        private readonly IHtmlSanitizer _htmlSanitizer;
        private readonly HtmlImageSourceParser _imageSourceParser;
        private readonly IImageUrlGenerator _imageUrlGenerator;
        private readonly HtmlLocalLinkParser _localLinkParser;
        private readonly IHtmlMacroParameterParser _macroParameterParser;
        private readonly RichTextEditorPastedImages _pastedImages;

        public RichTextPropertyValueEditor(
            DataEditorAttribute attribute,
            IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
            ILocalizedTextService localizedTextService,
            IShortStringHelper shortStringHelper,
            HtmlImageSourceParser imageSourceParser,
            HtmlLocalLinkParser localLinkParser,
            RichTextEditorPastedImages pastedImages,
            IImageUrlGenerator imageUrlGenerator,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            IHtmlSanitizer htmlSanitizer,
            IHtmlMacroParameterParser macroParameterParser)
            : base(localizedTextService, shortStringHelper, jsonSerializer, ioHelper, attribute)
        {
            _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
            _imageSourceParser = imageSourceParser;
            _localLinkParser = localLinkParser;
            _pastedImages = pastedImages;
            _imageUrlGenerator = imageUrlGenerator;
            _htmlSanitizer = htmlSanitizer;
            _macroParameterParser = macroParameterParser;
        }

        [Obsolete("Use the constructor which takes an HtmlMacroParameterParser instead")]
        public RichTextPropertyValueEditor(
            DataEditorAttribute attribute,
            IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
            ILocalizedTextService localizedTextService,
            IShortStringHelper shortStringHelper,
            HtmlImageSourceParser imageSourceParser,
            HtmlLocalLinkParser localLinkParser,
            RichTextEditorPastedImages pastedImages,
            IImageUrlGenerator imageUrlGenerator,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            IHtmlSanitizer htmlSanitizer)
            : this(
                attribute,
                backOfficeSecurityAccessor,
                localizedTextService,
                shortStringHelper,
                imageSourceParser,
                localLinkParser,
                pastedImages,
                imageUrlGenerator,
                jsonSerializer,
                ioHelper,
                htmlSanitizer,
                StaticServiceProvider.Instance.GetRequiredService<IHtmlMacroParameterParser>())
        {
        }

        /// <inheritdoc />
        public override object? Configuration
        {
            get => base.Configuration;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (!(value is RichTextConfiguration configuration))
                {
                    throw new ArgumentException(
                        $"Expected a {typeof(RichTextConfiguration).Name} instance, but got {value.GetType().Name}.",
                        nameof(value));
                }

                base.Configuration = value;

                HideLabel = configuration.HideLabel;
            }
        }

        /// <summary>
        ///     Resolve references from <see cref="IDataValueEditor" /> values
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public IEnumerable<UmbracoEntityReference> GetReferences(object? value)
        {
            var asString = value == null ? string.Empty : value is string str ? str : value.ToString()!;

            foreach (Udi udi in _imageSourceParser.FindUdisFromDataAttributes(asString))
            {
                yield return new UmbracoEntityReference(udi);
            }

            foreach (Udi? udi in _localLinkParser.FindUdisFromLocalLinks(asString))
            {
                if (udi is not null)
                {
                    yield return new UmbracoEntityReference(udi);
                }
            }

            // TODO: Detect Macros too ... but we can save that for a later date, right now need to do media refs
            // UPDATE: We are getting the Macros in 'FindUmbracoEntityReferencesFromEmbeddedMacros' - perhaps we just return the macro Udis here too or do they need their own relationAlias?
            foreach (UmbracoEntityReference umbracoEntityReference in _macroParameterParser
                         .FindUmbracoEntityReferencesFromEmbeddedMacros(asString))
            {
                yield return umbracoEntityReference;
            }
        }

        /// <summary>
        ///     Format the data for the editor
        /// </summary>
        /// <param name="property"></param>
        /// <param name="culture"></param>
        /// <param name="segment"></param>
        public override object? ToEditor(IProperty property, string? culture = null, string? segment = null)
        {
            var val = property.GetValue(culture, segment);
            if (val == null)
            {
                return null;
            }

            var propertyValueWithMediaResolved = _imageSourceParser.EnsureImageSources(val.ToString()!);
            var parsed = MacroTagParser.FormatRichTextPersistedDataForEditor(
                propertyValueWithMediaResolved,
                new Dictionary<string, string>());
            return parsed;
        }

        /// <summary>
        ///     Format the data for persistence
        /// </summary>
        /// <param name="editorValue"></param>
        /// <param name="currentValue"></param>
        /// <returns></returns>
        public override object? FromEditor(ContentPropertyData editorValue, object? currentValue)
        {
            if (editorValue.Value == null)
            {
                return null;
            }

            var userId = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.Id ??
                         Constants.Security.SuperUserId;

            var config = editorValue.DataTypeConfiguration as RichTextConfiguration;
            GuidUdi? mediaParent = config?.MediaParentId;
            Guid mediaParentId = mediaParent == null ? Guid.Empty : mediaParent.Guid;

            if (string.IsNullOrWhiteSpace(editorValue.Value.ToString()))
            {
                return null;
            }

            var parseAndSavedTempImages =
                _pastedImages.FindAndPersistPastedTempImages(editorValue.Value.ToString()!, mediaParentId, userId, _imageUrlGenerator);
            var editorValueWithMediaUrlsRemoved = _imageSourceParser.RemoveImageSources(parseAndSavedTempImages);
            var parsed = MacroTagParser.FormatRichTextContentForPersistence(editorValueWithMediaUrlsRemoved);
            var sanitized = _htmlSanitizer.Sanitize(parsed);

            return sanitized.NullOrWhiteSpaceAsNull();
        }
    }

    internal class RichTextPropertyIndexValueFactory : IPropertyIndexValueFactory
    {
        public IEnumerable<KeyValuePair<string, IEnumerable<object?>>> GetIndexValues(IProperty property, string? culture, string? segment, bool published)
        {
            var val = property.GetValue(culture, segment, published);

            if (!(val is string strVal))
            {
                yield break;
            }

            // index the stripped HTML values
            yield return new KeyValuePair<string, IEnumerable<object?>>(
                property.Alias,
                new object[] { strVal.StripHtml() });

            // store the raw value
            yield return new KeyValuePair<string, IEnumerable<object?>>(
                $"{UmbracoExamineFieldNames.RawFieldPrefix}{property.Alias}", new object[] { strVal });
        }
    }
}
