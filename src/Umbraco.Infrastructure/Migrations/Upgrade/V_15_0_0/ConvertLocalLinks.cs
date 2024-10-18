using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Templates;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0;

public class ConvertLocalLinks : MigrationBase
{
    private readonly IUmbracoContextFactory _umbracoContextFactory;
    private readonly HtmlLocalLinkParser _localLinkParser;
    private readonly IContentTypeService _contentTypeService;
    private readonly ILogger<ConvertLocalLinks> _logger;
    private readonly IDataTypeService _dataTypeService;
    private readonly ILanguageService _languageService;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IIdKeyMap _idKeyMap;

    private string[] propertyEditorAliasses =
    [
        Constants.PropertyEditors.Aliases.TinyMce,
        Constants.PropertyEditors.Aliases.RichText,
        Constants.PropertyEditors.Aliases.BlockList,
        Constants.PropertyEditors.Aliases.BlockGrid
    ];

    public ConvertLocalLinks(
        IMigrationContext context,
        IUmbracoContextFactory umbracoContextFactory,
        HtmlLocalLinkParser localLinkParser,
        IContentTypeService contentTypeService,
        ILogger<ConvertLocalLinks> logger,
        IDataTypeService dataTypeService,
        ILanguageService languageService,
        IJsonSerializer jsonSerializer,
        IIdKeyMap idKeyMap)
        : base(context)
    {
        _umbracoContextFactory = umbracoContextFactory;
        _localLinkParser = localLinkParser;
        _contentTypeService = contentTypeService;
        _logger = logger;
        _dataTypeService = dataTypeService;
        _languageService = languageService;
        _jsonSerializer = jsonSerializer;
        _idKeyMap = idKeyMap;
    }

    protected override void Migrate()
    {
        using UmbracoContextReference umbracoContextReference = _umbracoContextFactory.EnsureUmbracoContext();
        var languagesById = _languageService.GetAllAsync().GetAwaiter().GetResult()
            .ToDictionary(language => language.Id);
        IContentType[] allContentTypes = _contentTypeService.GetAll().ToArray();
        var relevantPropertyEditors = allContentTypes
            .SelectMany(ct => ct.PropertyTypes)
            .Where(pt => propertyEditorAliasses.Contains(pt.PropertyEditorAlias))
            .GroupBy(pt => pt.PropertyEditorAlias)
            .ToDictionary(group => group.Key, group => group.ToArray());

        foreach (var propertyEditorAlias in propertyEditorAliasses)
        {
            if (relevantPropertyEditors.TryGetValue(propertyEditorAlias, out IPropertyType[]? propertyTypes) is false)
            {
                continue;
            }

            _logger.LogInformation(
                "Migration starting for all properties of type: {propertyEditorAlias}",
                propertyEditorAlias);
            if (ProcessPropertyTypes(propertyTypes, languagesById))
            {
                _logger.LogInformation(
                    "Migration succeeded for all properties of type: {propertyEditorAlias}",
                    propertyEditorAlias);
            }
            else
            {
                _logger.LogError(
                    "Migration failed for one or more properties of type: {propertyEditorAlias}",
                    propertyEditorAlias);
            }
        }
    }

    private bool ProcessPropertyTypes(IPropertyType[] propertyTypes, IDictionary<int, ILanguage> languagesById)
    {
        foreach (IPropertyType propertyType in propertyTypes)
        {
            IDataType dataType = _dataTypeService.GetAsync(propertyType.DataTypeKey).GetAwaiter().GetResult()
                                 ?? throw new InvalidOperationException("The data type could not be fetched.");

            IDataValueEditor valueEditor = dataType.Editor?.GetValueEditor()
                                           ?? throw new InvalidOperationException(
                                               "The data type value editor could not be fetched.");

            Sql<ISqlContext> sql = Sql($"""
                                        select pd.* from umbracoPropertyData pd
                                        left join umbracoContentVersion cv on pd.versionId = cv.id
                                        left join umbracoDocumentVersion dv on dv.id = cv.id
                                        where (cv.current = 1 or dv.published = 1)
                                        and pd.PropertyTypeId = {propertyType.Id}
                                        """);

            List<PropertyDataDto> propertyDataDtos = Database.Fetch<PropertyDataDto>(sql);
            if (propertyDataDtos.Any() is false)
            {
                continue;
            }

            foreach (PropertyDataDto propertyDataDto in propertyDataDtos)
            {
                if (ProcessPropertyDataDto(propertyDataDto, propertyType, languagesById, valueEditor))
                {
                    Database.Update(propertyDataDto);
                }
            }
        }

        return true;
    }

    private bool ProcessPropertyDataDto(PropertyDataDto propertyDataDto, IPropertyType propertyType,
        IDictionary<int, ILanguage> languagesById, IDataValueEditor valueEditor)
    {
        // NOTE: some old property data DTOs can have variance defined, even if the property type no longer varies
        var culture = propertyType.VariesByCulture()
                      && propertyDataDto.LanguageId.HasValue
                      && languagesById.TryGetValue(propertyDataDto.LanguageId.Value, out ILanguage? language)
            ? language.IsoCode
            : null;

        if (culture is null && propertyType.VariesByCulture())
        {
            // if we end up here, the property DTO is bound to a language that no longer exists. this is an error scenario,
            // and we can't really handle it in any other way than logging; in all likelihood this is an old property version,
            // and it won't cause any runtime issues
            _logger.LogWarning(
                "    - property data with id: {propertyDataId} references a language that does not exist - language id: {languageId} (property type: {propertyTypeName}, id: {propertyTypeId}, alias: {propertyTypeAlias})",
                propertyDataDto.Id,
                propertyDataDto.LanguageId,
                propertyType.Name,
                propertyType.Id,
                propertyType.Alias);
            return false;
        }

        var segment = propertyType.VariesBySegment() ? propertyDataDto.Segment : null;
        var property = new Property(propertyType);
        property.SetValue(propertyDataDto.Value, culture, segment);
        var toEditorValue = valueEditor.ToEditor(property, culture, segment);
        ProcessToEditorValue(toEditorValue);

        var editorValue = _jsonSerializer.Serialize(toEditorValue);
        var dbValue = valueEditor.FromEditor(new ContentPropertyData(editorValue, null), null);
        if (dbValue is not string stringValue || stringValue.DetectIsJson() is false)
        {
            _logger.LogError(
                "    - value editor did not yield a valid JSON string as FromEditor value property data with id: {propertyDataId} (property type: {propertyTypeName}, id: {propertyTypeId}, alias: {propertyTypeAlias})",
                propertyDataDto.Id,
                propertyType.Name,
                propertyType.Id,
                propertyType.Alias);
            return false;
        }

        propertyDataDto.TextValue = stringValue;
        return true;
    }

    private bool ProcessToEditorValue(object? editorValue)
    {
        switch (editorValue)
        {
            case RichTextEditorValue richTextValue:
                return ProcessRichText(richTextValue);
            case BlockValue blockValue:
                return ProcessBlocks(blockValue);
        }

        return false;
    }

    private bool ProcessRichText(RichTextEditorValue richTextValue)
    {
        bool hasChanged = false;

        var newMarkup = ProcessStringValue(richTextValue.Markup);
        if (newMarkup.Equals(richTextValue.Markup) == false)
        {
            hasChanged = true;
            richTextValue.Markup = newMarkup;
        }

        if (richTextValue.Blocks is null)
        {
            return hasChanged;
        }

        foreach (BlockItemData blockItemData in richTextValue.Blocks.ContentData)
        {
            foreach (BlockPropertyValue blockPropertyValue in blockItemData.Values)
            {
                if (ProcessToEditorValue(blockPropertyValue.Value))
                {
                    hasChanged = true;
                }
            }
        }

        return hasChanged;
    }

    private bool ProcessBlocks(BlockValue blockValue)
    {
        bool hasChanged = false;

        foreach (BlockItemData blockItemData in blockValue.ContentData)
        {
            foreach (BlockPropertyValue blockPropertyValue in blockItemData.Values)
            {
                if (ProcessToEditorValue(blockPropertyValue.Value))
                {
                    hasChanged = true;
                }
            }
        }

        return hasChanged;
    }

    private string ProcessStringValue(string input)
    {
        // find all legacy tags
        IEnumerable<HtmlLocalLinkParser.LocalLinkTag> tags = _localLinkParser.FindLegacyLocalLinkIds(input);

        foreach (HtmlLocalLinkParser.LocalLinkTag tag in tags)
        {
            string newTagHref;
            if (tag.Udi is not null)
            {
                newTagHref = $" type=\"{tag.Udi.EntityType}\" "
                             + tag.TagHref.Replace(tag.Udi.ToString(), tag.Udi.Guid.ToString());
            }
            else if (tag.IntId is not null)
            {
                // try to get the key and type from the int, else do nothing
                (Guid Key, string EntityType)? conversionResult = CreateIntBasedTag(tag.IntId.Value);
                if (conversionResult is null)
                {
                    continue;
                }

                newTagHref = $" type=\"{conversionResult.Value.EntityType}\" "
                             + tag.TagHref.Replace(tag.IntId.Value.ToString(), conversionResult.Value.Key.ToString());
            }
            else
            {
                // tag does not contain enough information to convert
                continue;
            }

            input = input.Replace(tag.TagHref, newTagHref);
        }

        return input;
    }

    private (Guid Key, string EntityType)? CreateIntBasedTag(int id)
    {
        // very old data, best effort replacement
        Attempt<Guid> documentAttempt = _idKeyMap.GetKeyForId(id, UmbracoObjectTypes.Document);
        if (documentAttempt.Success)
        {
            return (Key: documentAttempt.Result, EntityType: UmbracoObjectTypes.Document.ToString());
        }

        Attempt<Guid> mediaAttempt = _idKeyMap.GetKeyForId(id, UmbracoObjectTypes.Media);
        if (mediaAttempt.Success)
        {
            return (Key: mediaAttempt.Result, EntityType: UmbracoObjectTypes.Media.ToString());
        }

        return null;
    }
}
