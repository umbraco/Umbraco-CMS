// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Templates;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Infrastructure.Extensions;
using Umbraco.Cms.Infrastructure.Macros;
using Umbraco.Cms.Infrastructure.Templates;
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
    internal class RichTextPropertyValueEditor : BlockValuePropertyValueEditorBase
    {
        private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
        private readonly IHtmlSanitizer _htmlSanitizer;
        private readonly HtmlImageSourceParser _imageSourceParser;
        private readonly HtmlLocalLinkParser _localLinkParser;
        private readonly IHtmlMacroParameterParser _macroParameterParser;
        private readonly RichTextEditorPastedImages _pastedImages;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IContentTypeService _contentTypeService;
        private readonly ILogger<RichTextPropertyValueEditor> _logger;

        public RichTextPropertyValueEditor(
            DataEditorAttribute attribute,
            PropertyEditorCollection propertyEditors,
            IDataTypeService dataTypeService,
            ILogger<RichTextPropertyValueEditor> logger,
            IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
            ILocalizedTextService localizedTextService,
            IShortStringHelper shortStringHelper,
            HtmlImageSourceParser imageSourceParser,
            HtmlLocalLinkParser localLinkParser,
            RichTextEditorPastedImages pastedImages,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            IHtmlSanitizer htmlSanitizer,
            IHtmlMacroParameterParser macroParameterParser,
            IContentTypeService contentTypeService,
            IPropertyValidationService propertyValidationService)
            : base(attribute, propertyEditors, dataTypeService, localizedTextService, logger, shortStringHelper, jsonSerializer, ioHelper)
        {
            _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
            _imageSourceParser = imageSourceParser;
            _localLinkParser = localLinkParser;
            _pastedImages = pastedImages;
            _htmlSanitizer = htmlSanitizer;
            _macroParameterParser = macroParameterParser;
            _contentTypeService = contentTypeService;
            _jsonSerializer = jsonSerializer;
            _logger = logger;

            Validators.Add(new RichTextEditorBlockValidator(propertyValidationService, CreateBlockEditorValues(), contentTypeService, jsonSerializer, logger));
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
        public override IEnumerable<UmbracoEntityReference> GetReferences(object? value)
        {
            RichTextEditorValue? richTextEditorValue = TryParseEditorValue(value);
            if (richTextEditorValue is null)
            {
                return Array.Empty<UmbracoEntityReference>();
            }

            var references = new List<UmbracoEntityReference>();

            // image references from markup
            references.AddRange(_imageSourceParser
                .FindUdisFromDataAttributes(richTextEditorValue.Markup)
                .Select(udi => new UmbracoEntityReference(udi)));

            // local link references from markup
            references.AddRange(_localLinkParser
                .FindUdisFromLocalLinks(richTextEditorValue.Markup)
                .WhereNotNull()
                .Select(udi => new UmbracoEntityReference(udi)));

            // TODO: Detect Macros too ... but we can save that for a later date, right now need to do media refs
            // UPDATE: We are getting the Macros in 'FindUmbracoEntityReferencesFromEmbeddedMacros' - perhaps we just return the macro Udis here too or do they need their own relationAlias?
            references.AddRange(_macroParameterParser.FindUmbracoEntityReferencesFromEmbeddedMacros(richTextEditorValue.Markup));

            // references from blocks
            if (richTextEditorValue.Blocks is not null)
            {
                references.AddRange(GetBlockValueReferences(richTextEditorValue.Blocks));
            }

            return references;
        }

        public override IEnumerable<ITag> GetTags(object? value, object? dataTypeConfiguration, int? languageId)
        {
            RichTextEditorValue? richTextEditorValue = TryParseEditorValue(value);
            return richTextEditorValue?.Blocks is not null
                ? GetBlockValueTags(richTextEditorValue.Blocks, languageId)
                : Array.Empty<ITag>();
        }

        /// <summary>
        ///     Format the data for the editor
        /// </summary>
        /// <param name="property"></param>
        /// <param name="culture"></param>
        /// <param name="segment"></param>
        public override object? ToEditor(IProperty property, string? culture = null, string? segment = null)
        {
            var value = property.GetValue(culture, segment);
            RichTextEditorValue? richTextEditorValue = TryParseEditorValue(value);
            if (richTextEditorValue is null)
            {
                return null;
            }

            var propertyValueWithMediaResolved = _imageSourceParser.EnsureImageSources(richTextEditorValue.Markup);
            var parsed = MacroTagParser.FormatRichTextPersistedDataForEditor(
                propertyValueWithMediaResolved,
                new Dictionary<string, string>());
            richTextEditorValue.Markup = parsed;

            // return json convertable object
            return CleanAndMapBlocks(richTextEditorValue, blockValue => MapBlockValueToEditor(property, blockValue));
        }

        /// <summary>
        ///     Format the data for persistence
        /// </summary>
        /// <param name="editorValue"></param>
        /// <param name="currentValue"></param>
        /// <returns></returns>
        public override object? FromEditor(ContentPropertyData editorValue, object? currentValue)
        {
            RichTextEditorValue? richTextEditorValue = TryParseEditorValue(editorValue.Value);
            if (richTextEditorValue is null)
            {
                return null;
            }

            var userId = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.Id ??
                         Constants.Security.SuperUserId;

            var config = editorValue.DataTypeConfiguration as RichTextConfiguration;
            GuidUdi? mediaParent = config?.MediaParentId;
            Guid mediaParentId = mediaParent == null ? Guid.Empty : mediaParent.Guid;

            if (string.IsNullOrWhiteSpace(richTextEditorValue.Markup))
            {
                return null;
            }

            var parseAndSaveBase64Images = _pastedImages.FindAndPersistEmbeddedImages(
                richTextEditorValue.Markup, mediaParentId, userId);
            var parseAndSavedTempImages =
                _pastedImages.FindAndPersistPastedTempImages(parseAndSaveBase64Images, mediaParentId, userId);
            var editorValueWithMediaUrlsRemoved = _imageSourceParser.RemoveImageSources(parseAndSavedTempImages);
            var parsed = MacroTagParser.FormatRichTextContentForPersistence(editorValueWithMediaUrlsRemoved);
            var sanitized = _htmlSanitizer.Sanitize(parsed);

            richTextEditorValue.Markup = sanitized.NullOrWhiteSpaceAsNull() ?? string.Empty;

            RichTextEditorValue cleanedUpRichTextEditorValue = CleanAndMapBlocks(richTextEditorValue, MapBlockValueFromEditor);

            // return json
            return _jsonSerializer.Serialize(cleanedUpRichTextEditorValue);
        }

        private RichTextEditorValue? TryParseEditorValue(object? value)
            => value.TryParseRichTextEditorValue(_jsonSerializer, _logger);

        private RichTextEditorValue CleanAndMapBlocks(RichTextEditorValue richTextEditorValue, Action<BlockValue> handleMapping)
        {
            if (richTextEditorValue.Blocks is null)
            {
                // no blocks defined, store empty block value
                return MarkupWithEmptyBlocks();
            }

            try
            {
                BlockEditorValues blockEditorValues = CreateBlockEditorValues();
                BlockEditorData? blockEditorData = blockEditorValues.ConvertAndClean(richTextEditorValue.Blocks);

                if (blockEditorData is not null)
                {
                    handleMapping(blockEditorData.BlockValue);
                    return new RichTextEditorValue
                    {
                        Markup = richTextEditorValue.Markup, Blocks = blockEditorData.BlockValue
                    };
                }
            }
            catch (JsonSerializationException exception)
            {
                _logger.LogError(exception, "Could not parse block editor value, see exception for details.");
            }

            // could not deserialize the blocks or handle the mapping, store empty block value
            return MarkupWithEmptyBlocks();

            RichTextEditorValue MarkupWithEmptyBlocks() => new()
            {
                Markup = richTextEditorValue.Markup, Blocks = new BlockValue()
            };
        }

        private BlockEditorValues CreateBlockEditorValues()
            => new(new RichTextEditorBlockDataConverter(), _contentTypeService, _logger);
    }

    internal class RichTextPropertyIndexValueFactory : IPropertyIndexValueFactory
    {
        public IEnumerable<KeyValuePair<string, IEnumerable<object?>>> GetIndexValues(IProperty property, string? culture, string? segment, bool published, IEnumerable<string> availableCultures)
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

        [Obsolete("Use the overload with the 'availableCultures' parameter instead, scheduled for removal in v14")]
        public IEnumerable<KeyValuePair<string, IEnumerable<object?>>> GetIndexValues(IProperty property, string? culture, string? segment, bool published)
            => GetIndexValues(property, culture, segment, published, Enumerable.Empty<string>());
    }
}
