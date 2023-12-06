// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
    private readonly IEditorConfigurationParser _editorConfigurationParser;
    private readonly IIOHelper _ioHelper;
    private readonly IRichTextPropertyIndexValueFactory _richTextPropertyIndexValueFactory;

    [Obsolete("Use the constructor which takes an IHtmlMacroParameterParser instead. Will be removed in V15.")]
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

    [Obsolete("Use the constructor which takes an IHtmlMacroParameterParser instead. Will be removed in V15.")]
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

    [Obsolete($"Use the constructor which accepts an {nameof(IRichTextPropertyIndexValueFactory)} parameter. Will be removed in V15.")]
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
        : this(
            dataValueEditorFactory,
            backOfficeSecurityAccessor,
            imageSourceParser,
            localLinkParser,
            pastedImages,
            ioHelper,
            imageUrlGenerator,
            macroParameterParser,
            editorConfigurationParser,
            StaticServiceProvider.Instance.GetRequiredService<IRichTextPropertyIndexValueFactory>())
    {
    }

    [Obsolete($"Use the non-obsolete constructor. Will be removed in V15.")]
    public RichTextPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        HtmlImageSourceParser imageSourceParser,
        HtmlLocalLinkParser localLinkParser,
        RichTextEditorPastedImages pastedImages,
        IIOHelper ioHelper,
        IImageUrlGenerator imageUrlGenerator,
        IHtmlMacroParameterParser macroParameterParser,
        IEditorConfigurationParser editorConfigurationParser,
        IRichTextPropertyIndexValueFactory richTextPropertyIndexValueFactory)
        : this(
            dataValueEditorFactory,
            editorConfigurationParser,
            ioHelper,
            richTextPropertyIndexValueFactory)
    {
    }

    /// <summary>
    ///     The constructor will setup the property editor based on the attribute if one is found.
    /// </summary>
    public RichTextPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IEditorConfigurationParser editorConfigurationParser,
        IIOHelper ioHelper,
        IRichTextPropertyIndexValueFactory richTextPropertyIndexValueFactory)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        _richTextPropertyIndexValueFactory = richTextPropertyIndexValueFactory;
        _editorConfigurationParser = editorConfigurationParser;
        SupportsReadOnly = true;
    }

    public override IPropertyIndexValueFactory PropertyIndexValueFactory => _richTextPropertyIndexValueFactory;

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
            if (TryParseEditorValue(value, out RichTextEditorValue? richTextEditorValue) is false)
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
                BlockEditorData? blockEditorData = ConvertAndClean(richTextEditorValue.Blocks);
                if (blockEditorData is not null)
                {
                    references.AddRange(GetBlockValueReferences(blockEditorData.BlockValue));
                }
            }

            return references;
        }

        public override IEnumerable<ITag> GetTags(object? value, object? dataTypeConfiguration, int? languageId)
        {
            if (TryParseEditorValue(value, out RichTextEditorValue? richTextEditorValue) is false || richTextEditorValue.Blocks is null)
            {
                return Array.Empty<ITag>();
            }

            BlockEditorData? blockEditorData = ConvertAndClean(richTextEditorValue.Blocks);
            if (blockEditorData is null)
            {
                return Array.Empty<ITag>();
            }

            return GetBlockValueTags(blockEditorData.BlockValue, languageId);
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
            if (TryParseEditorValue(value, out RichTextEditorValue? richTextEditorValue) is false)
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
            if (TryParseEditorValue(editorValue.Value, out RichTextEditorValue? richTextEditorValue) is false)
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
            return RichTextPropertyEditorHelper.SerializeRichTextEditorValue(cleanedUpRichTextEditorValue, _jsonSerializer);
        }

        private bool TryParseEditorValue(object? value, [NotNullWhen(true)] out RichTextEditorValue? richTextEditorValue)
            => RichTextPropertyEditorHelper.TryParseRichTextEditorValue(value, _jsonSerializer, _logger, out richTextEditorValue);

        private RichTextEditorValue CleanAndMapBlocks(RichTextEditorValue richTextEditorValue, Action<BlockValue> handleMapping)
        {
            if (richTextEditorValue.Blocks is null)
            {
                // no blocks defined, store empty block value
                return MarkupWithEmptyBlocks();
            }

            BlockEditorData? blockEditorData = ConvertAndClean(richTextEditorValue.Blocks);

            if (blockEditorData is not null)
            {
                handleMapping(blockEditorData.BlockValue);
                return new RichTextEditorValue
                {
                    Markup = richTextEditorValue.Markup, Blocks = blockEditorData.BlockValue
                };
            }

            // could not deserialize the blocks or handle the mapping, store empty block value
            return MarkupWithEmptyBlocks();

            RichTextEditorValue MarkupWithEmptyBlocks() => new()
            {
                Markup = richTextEditorValue.Markup, Blocks = new BlockValue()
            };
        }

        private BlockEditorData? ConvertAndClean(BlockValue blockValue)
        {
            BlockEditorValues blockEditorValues = CreateBlockEditorValues();
            return blockEditorValues.ConvertAndClean(blockValue);
        }

        private BlockEditorValues CreateBlockEditorValues()
            => new(new RichTextEditorBlockDataConverter(), _contentTypeService, _logger);
    }
}
