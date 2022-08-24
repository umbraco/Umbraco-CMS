using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models.Mapping;

/// <summary>
///     Defines mappings for content/media/members type mappings
/// </summary>
public class ContentTypeMapDefinition : IMapDefinition
{
    private readonly CommonMapper _commonMapper;
    private readonly IContentTypeService _contentTypeService;
    private readonly IDataTypeService _dataTypeService;
    private readonly IFileService _fileService;
    private readonly GlobalSettings _globalSettings;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly ILogger<ContentTypeMapDefinition> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IMemberTypeService _memberTypeService;
    private readonly PropertyEditorCollection _propertyEditors;
    private readonly IShortStringHelper _shortStringHelper;
    private ContentSettings _contentSettings;

    [Obsolete("Use ctor with all params injected")]
    public ContentTypeMapDefinition(
        CommonMapper commonMapper,
        PropertyEditorCollection propertyEditors,
        IDataTypeService dataTypeService,
        IFileService fileService,
        IContentTypeService contentTypeService,
        IMediaTypeService mediaTypeService,
        IMemberTypeService memberTypeService,
        ILoggerFactory loggerFactory,
        IShortStringHelper shortStringHelper,
        IOptions<GlobalSettings> globalSettings,
        IHostingEnvironment hostingEnvironment)
        : this(
            commonMapper,
            propertyEditors,
            dataTypeService,
            fileService,
            contentTypeService,
            mediaTypeService,
            memberTypeService,
            loggerFactory,
            shortStringHelper,
            globalSettings,
            hostingEnvironment,
            StaticServiceProvider.Instance.GetRequiredService<IOptionsMonitor<ContentSettings>>())
    {
    }

    public ContentTypeMapDefinition(
        CommonMapper commonMapper,
        PropertyEditorCollection propertyEditors,
        IDataTypeService dataTypeService,
        IFileService fileService,
        IContentTypeService contentTypeService,
        IMediaTypeService mediaTypeService,
        IMemberTypeService memberTypeService,
        ILoggerFactory loggerFactory,
        IShortStringHelper shortStringHelper,
        IOptions<GlobalSettings> globalSettings,
        IHostingEnvironment hostingEnvironment,
        IOptionsMonitor<ContentSettings> contentSettings)
    {
        _commonMapper = commonMapper;
        _propertyEditors = propertyEditors;
        _dataTypeService = dataTypeService;
        _fileService = fileService;
        _contentTypeService = contentTypeService;
        _mediaTypeService = mediaTypeService;
        _memberTypeService = memberTypeService;
        _loggerFactory = loggerFactory;
        _logger = _loggerFactory.CreateLogger<ContentTypeMapDefinition>();
        _shortStringHelper = shortStringHelper;
        _globalSettings = globalSettings.Value;
        _hostingEnvironment = hostingEnvironment;

        _contentSettings = contentSettings.CurrentValue;
        contentSettings.OnChange(x => _contentSettings = x);
    }

    public static Udi? MapContentTypeUdi(IContentTypeComposition source)
    {
        if (source == null)
        {
            return null;
        }

        string udiType;
        switch (source)
        {
            case IMemberType _:
                udiType = Constants.UdiEntityType.MemberType;
                break;
            case IMediaType _:
                udiType = Constants.UdiEntityType.MediaType;
                break;
            case IContentType _:
                udiType = Constants.UdiEntityType.DocumentType;
                break;
            default:
                throw new PanicException($"Source is of type {source.GetType()} which isn't supported here");
        }

        return Udi.Create(udiType, source.Key);
    }

    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<DocumentTypeSave, IContentType>(
            (source, context) => new ContentType(_shortStringHelper, source.ParentId), Map);
        mapper.Define<MediaTypeSave, IMediaType>(
            (source, context) => new MediaType(_shortStringHelper, source.ParentId), Map);
        mapper.Define<MemberTypeSave, IMemberType>(
            (source, context) => new MemberType(_shortStringHelper, source.ParentId), Map);

        mapper.Define<IContentType, DocumentTypeDisplay>((source, context) => new DocumentTypeDisplay(), Map);
        mapper.Define<IMediaType, MediaTypeDisplay>((source, context) => new MediaTypeDisplay(), Map);
        mapper.Define<IMemberType, MemberTypeDisplay>((source, context) => new MemberTypeDisplay(), Map);

        mapper.Define<PropertyTypeBasic, IPropertyType>(
            (source, context) =>
            {
                IDataType? dataType = _dataTypeService.GetDataType(source.DataTypeId);
                if (dataType == null)
                {
                    throw new NullReferenceException("No data type found with id " + source.DataTypeId);
                }

                return new PropertyType(_shortStringHelper, dataType, source.Alias);
            },
            Map);

        // TODO: isPublishing in ctor?
        mapper.Define<PropertyGroupBasic<PropertyTypeBasic>, PropertyGroup>(
            (source, context) => new PropertyGroup(false), Map);
        mapper.Define<PropertyGroupBasic<MemberPropertyTypeBasic>, PropertyGroup>(
            (source, context) => new PropertyGroup(false), Map);

        mapper.Define<IContentTypeComposition, ContentTypeBasic>((source, context) => new ContentTypeBasic(), Map);
        mapper.Define<IContentType, ContentTypeBasic>((source, context) => new ContentTypeBasic(), Map);
        mapper.Define<IMediaType, ContentTypeBasic>((source, context) => new ContentTypeBasic(), Map);
        mapper.Define<IMemberType, ContentTypeBasic>((source, context) => new ContentTypeBasic(), Map);

        mapper.Define<DocumentTypeSave, DocumentTypeDisplay>((source, context) => new DocumentTypeDisplay(), Map);
        mapper.Define<MediaTypeSave, MediaTypeDisplay>((source, context) => new MediaTypeDisplay(), Map);
        mapper.Define<MemberTypeSave, MemberTypeDisplay>((source, context) => new MemberTypeDisplay(), Map);

        mapper.Define<PropertyGroupBasic<PropertyTypeBasic>, PropertyGroupDisplay<PropertyTypeDisplay>>(
            (source, context) => new PropertyGroupDisplay<PropertyTypeDisplay>(), Map);
        mapper.Define<PropertyGroupBasic<MemberPropertyTypeBasic>, PropertyGroupDisplay<MemberPropertyTypeDisplay>>(
            (source, context) => new PropertyGroupDisplay<MemberPropertyTypeDisplay>(), Map);

        mapper.Define<PropertyTypeBasic, PropertyTypeDisplay>((source, context) => new PropertyTypeDisplay(), Map);
        mapper.Define<MemberPropertyTypeBasic, MemberPropertyTypeDisplay>(
            (source, context) => new MemberPropertyTypeDisplay(), Map);
    }

    private static void MapHistoryCleanup(DocumentTypeSave source, IContentTypeWithHistoryCleanup target)
    {
        // If source history cleanup is null we don't have to map all properties
        if (source.HistoryCleanup is null)
        {
            target.HistoryCleanup = null;
            return;
        }

        // We need to reset the dirty properties, because it is otherwise true, just because the json serializer has set properties
        target.HistoryCleanup!.ResetDirtyProperties(false);
        if (target.HistoryCleanup.PreventCleanup != source.HistoryCleanup.PreventCleanup)
        {
            target.HistoryCleanup.PreventCleanup = source.HistoryCleanup.PreventCleanup;
        }

        if (target.HistoryCleanup.KeepAllVersionsNewerThanDays != source.HistoryCleanup.KeepAllVersionsNewerThanDays)
        {
            target.HistoryCleanup.KeepAllVersionsNewerThanDays = source.HistoryCleanup.KeepAllVersionsNewerThanDays;
        }

        if (target.HistoryCleanup.KeepLatestVersionPerDayForDays !=
            source.HistoryCleanup.KeepLatestVersionPerDayForDays)
        {
            target.HistoryCleanup.KeepLatestVersionPerDayForDays = source.HistoryCleanup.KeepLatestVersionPerDayForDays;
        }
    }

    // Umbraco.Code.MapAll -CreateDate -DeleteDate -UpdateDate
    // Umbraco.Code.MapAll -SupportsPublishing -Key -PropertyEditorAlias -ValueStorageType -Variations
    private static void Map(PropertyTypeBasic source, IPropertyType target, MapperContext context)
    {
        target.Name = source.Label;
        target.DataTypeId = source.DataTypeId;
        target.DataTypeKey = source.DataTypeKey;
        target.Mandatory = source.Validation?.Mandatory ?? false;
        target.MandatoryMessage = source.Validation?.MandatoryMessage;
        target.ValidationRegExp = source.Validation?.Pattern;
        target.ValidationRegExpMessage = source.Validation?.PatternMessage;
        target.SetVariesBy(ContentVariation.Culture, source.AllowCultureVariant);
        target.SetVariesBy(ContentVariation.Segment, source.AllowSegmentVariant);

        if (source.Id > 0)
        {
            target.Id = source.Id;
        }

        if (source.GroupId > 0)
        {
            if (target.PropertyGroupId?.Value != source.GroupId)
            {
                target.PropertyGroupId = new Lazy<int>(() => source.GroupId, false);
            }
        }

        target.Alias = source.Alias;
        target.Description = source.Description;
        target.SortOrder = source.SortOrder;
        target.LabelOnTop = source.LabelOnTop;
    }

    // Umbraco.Code.MapAll -CreateDate -UpdateDate -DeleteDate -Key -PropertyTypes
    private static void Map(PropertyGroupBasic<PropertyTypeBasic> source, PropertyGroup target, MapperContext context)
    {
        if (source.Id > 0)
        {
            target.Id = source.Id;
        }

        target.Key = source.Key;
        target.Type = source.Type;
        target.Name = source.Name;
        target.Alias = source.Alias;
        target.SortOrder = source.SortOrder;
    }

    // no MapAll - take care
    private void Map(DocumentTypeSave source, IContentType target, MapperContext context)
    {
        MapSaveToTypeBase<DocumentTypeSave, PropertyTypeBasic>(source, target, context);
        MapComposition(source, target, alias => _contentTypeService.Get(alias));

        if (target is IContentTypeWithHistoryCleanup targetWithHistoryCleanup)
        {
            MapHistoryCleanup(source, targetWithHistoryCleanup);
        }

        target.AllowedTemplates = source.AllowedTemplates?
            .Where(x => x != null)
            .Select(_fileService.GetTemplate)
            .WhereNotNull()
            .ToArray();

        target.SetDefaultTemplate(source.DefaultTemplate == null
            ? null
            : _fileService.GetTemplate(source.DefaultTemplate));
    }

    // no MapAll - take care
    private void Map(MediaTypeSave source, IMediaType target, MapperContext context)
    {
        MapSaveToTypeBase<MediaTypeSave, PropertyTypeBasic>(source, target, context);
        MapComposition(source, target, alias => _mediaTypeService.Get(alias));
    }

    // no MapAll - take care
    private void Map(MemberTypeSave source, IMemberType target, MapperContext context)
    {
        MapSaveToTypeBase<MemberTypeSave, MemberPropertyTypeBasic>(source, target, context);
        MapComposition(source, target, alias => _memberTypeService.Get(alias));

        foreach (MemberPropertyTypeBasic propertyType in source.Groups.SelectMany(x => x.Properties))
        {
            MemberPropertyTypeBasic localCopy = propertyType;
            IPropertyType? destProp =
                target.PropertyTypes.SingleOrDefault(x => x.Alias?.InvariantEquals(localCopy.Alias) ?? false);
            if (destProp == null)
            {
                continue;
            }

            target.SetMemberCanEditProperty(localCopy.Alias, localCopy.MemberCanEditProperty);
            target.SetMemberCanViewProperty(localCopy.Alias, localCopy.MemberCanViewProperty);
            target.SetIsSensitiveProperty(localCopy.Alias, localCopy.IsSensitiveData);
        }
    }

    // no MapAll - take care
    private void Map(IContentType source, DocumentTypeDisplay target, MapperContext context)
    {
        MapTypeToDisplayBase<DocumentTypeDisplay, PropertyTypeDisplay>(source, target);

        if (source is IContentTypeWithHistoryCleanup sourceWithHistoryCleanup)
        {
            target.HistoryCleanup = new HistoryCleanupViewModel
            {
                PreventCleanup = sourceWithHistoryCleanup.HistoryCleanup?.PreventCleanup ?? false,
                KeepAllVersionsNewerThanDays =
                    sourceWithHistoryCleanup.HistoryCleanup?.KeepAllVersionsNewerThanDays,
                KeepLatestVersionPerDayForDays =
                    sourceWithHistoryCleanup.HistoryCleanup?.KeepLatestVersionPerDayForDays,
                GlobalKeepAllVersionsNewerThanDays =
                    _contentSettings.ContentVersionCleanupPolicy.KeepAllVersionsNewerThanDays,
                GlobalKeepLatestVersionPerDayForDays =
                    _contentSettings.ContentVersionCleanupPolicy.KeepLatestVersionPerDayForDays,
                GlobalEnableCleanup = _contentSettings.ContentVersionCleanupPolicy.EnableCleanup,
            };
        }

        target.AllowCultureVariant = source.VariesByCulture();
        target.AllowSegmentVariant = source.VariesBySegment();
        target.ContentApps = _commonMapper.GetContentAppsForEntity(source);
        target.Variations = source.Variations;

        // sync templates
        if (source.AllowedTemplates is not null)
        {
            target.AllowedTemplates =
                context.MapEnumerable<ITemplate, EntityBasic>(source.AllowedTemplates).WhereNotNull();
        }

        if (source.DefaultTemplate != null)
        {
            target.DefaultTemplate = context.Map<EntityBasic>(source.DefaultTemplate);
        }

        // default listview
        target.ListViewEditorName = Constants.Conventions.DataTypes.ListViewPrefix + "Content";

        if (string.IsNullOrEmpty(source.Alias))
        {
            return;
        }

        var name = Constants.Conventions.DataTypes.ListViewPrefix + source.Alias;
        if (_dataTypeService.GetDataType(name) != null)
        {
            target.ListViewEditorName = name;
        }
    }

    // no MapAll - take care
    private void Map(IMediaType source, MediaTypeDisplay target, MapperContext context)
    {
        MapTypeToDisplayBase<MediaTypeDisplay, PropertyTypeDisplay>(source, target);

        // default listview
        target.ListViewEditorName = Constants.Conventions.DataTypes.ListViewPrefix + "Media";
        target.IsSystemMediaType = source.IsSystemMediaType();

        if (string.IsNullOrEmpty(source.Name))
        {
            return;
        }

        var name = Constants.Conventions.DataTypes.ListViewPrefix + source.Name;
        if (_dataTypeService.GetDataType(name) != null)
        {
            target.ListViewEditorName = name;
        }
    }

    // no MapAll - take care
    private void Map(IMemberType source, MemberTypeDisplay target, MapperContext context)
    {
        MapTypeToDisplayBase<MemberTypeDisplay, MemberPropertyTypeDisplay>(source, target);

        // map the MemberCanEditProperty,MemberCanViewProperty,IsSensitiveData
        foreach (IPropertyType propertyType in source.PropertyTypes)
        {
            IPropertyType localCopy = propertyType;
            MemberPropertyTypeDisplay? displayProp = target.Groups.SelectMany(dest => dest.Properties)
                .SingleOrDefault(dest => dest.Alias?.InvariantEquals(localCopy.Alias) ?? false);
            if (displayProp == null)
            {
                continue;
            }

            displayProp.MemberCanEditProperty = source.MemberCanEditProperty(localCopy.Alias);
            displayProp.MemberCanViewProperty = source.MemberCanViewProperty(localCopy.Alias);
            displayProp.IsSensitiveData = source.IsSensitiveProperty(localCopy.Alias);
        }
    }

    // Umbraco.Code.MapAll -Blueprints
    private void Map(IContentTypeBase source, ContentTypeBasic target, string entityType)
    {
        target.Udi = Udi.Create(entityType, source.Key);
        target.Alias = source.Alias;
        target.CreateDate = source.CreateDate;
        target.Description = source.Description;
        target.Icon = source.Icon;
        target.IconFilePath = target.IconIsClass
            ? string.Empty
            : $"{_globalSettings.GetBackOfficePath(_hostingEnvironment).EnsureEndsWith("/")}images/umbraco/{source.Icon}";

        target.Trashed = source.Trashed;
        target.Id = source.Id;
        target.IsContainer = source.IsContainer;
        target.IsElement = source.IsElement;
        target.Key = source.Key;
        target.Name = source.Name;
        target.ParentId = source.ParentId;
        target.Path = source.Path;
        target.Thumbnail = source.Thumbnail;
        target.ThumbnailFilePath = target.ThumbnailIsClass
            ? string.Empty
            : _hostingEnvironment.ToAbsolute("~/umbraco/images/thumbnails/" + source.Thumbnail);
        target.UpdateDate = source.UpdateDate;
        target.Variations = source.Variations;
    }

    // no MapAll - uses the IContentTypeBase map method, which has MapAll
    private void Map(IContentTypeComposition source, ContentTypeBasic target, MapperContext context) =>
        Map(source, target, Constants.UdiEntityType.MemberType);

    // no MapAll - uses the IContentTypeBase map method, which has MapAll
    private void Map(IContentType source, ContentTypeBasic target, MapperContext context) =>
        Map(source, target, Constants.UdiEntityType.DocumentType);

    // no MapAll - uses the IContentTypeBase map method, which has MapAll
    private void Map(IMediaType source, ContentTypeBasic target, MapperContext context) =>
        Map(source, target, Constants.UdiEntityType.MediaType);

    // no MapAll - uses the IContentTypeBase map method, which has MapAll
    private void Map(IMemberType source, ContentTypeBasic target, MapperContext context) =>
        Map(source, target, Constants.UdiEntityType.MemberType);

    // no MapAll - take care
    private void Map(DocumentTypeSave source, DocumentTypeDisplay target, MapperContext context)
    {
        MapTypeToDisplayBase<DocumentTypeSave, PropertyTypeBasic, DocumentTypeDisplay, PropertyTypeDisplay>(
            source,
            target,
            context);

        // sync templates
        IEnumerable<string?> destAllowedTemplateAliases = target.AllowedTemplates.Select(x => x.Alias);

        // if the dest is set and it's the same as the source, then don't change
        if (source.AllowedTemplates is not null &&
            destAllowedTemplateAliases.SequenceEqual(source.AllowedTemplates) == false)
        {
            IEnumerable<ITemplate>? templates = _fileService.GetTemplates(source.AllowedTemplates.ToArray());
            target.AllowedTemplates = source.AllowedTemplates
                .Select(x =>
                {
                    ITemplate? template = templates?.SingleOrDefault(t => t.Alias == x);
                    return template != null
                        ? context.Map<EntityBasic>(template)
                        : null;
                })
                .WhereNotNull()
                .ToArray();
        }

        if (source.DefaultTemplate.IsNullOrWhiteSpace() == false)
        {
            // if the dest is set and it's the same as the source, then don't change
            if (target.DefaultTemplate == null || source.DefaultTemplate != target.DefaultTemplate.Alias)
            {
                ITemplate? template = _fileService.GetTemplate(source.DefaultTemplate);
                target.DefaultTemplate = template == null ? null : context.Map<EntityBasic>(template);
            }
        }
        else
        {
            target.DefaultTemplate = null;
        }
    }

    // no MapAll - take care
    private void Map(MediaTypeSave source, MediaTypeDisplay target, MapperContext context) =>
        MapTypeToDisplayBase<MediaTypeSave, PropertyTypeBasic, MediaTypeDisplay, PropertyTypeDisplay>(
            source,
            target,
            context);

    // no MapAll - take care
    private void Map(MemberTypeSave source, MemberTypeDisplay target, MapperContext context) =>
        MapTypeToDisplayBase<MemberTypeSave, MemberPropertyTypeBasic, MemberTypeDisplay, MemberPropertyTypeDisplay>(
            source, target, context);

    // Umbraco.Code.MapAll -CreateDate -UpdateDate -DeleteDate -Key -PropertyTypes
    private static void Map(PropertyGroupBasic<MemberPropertyTypeBasic> source, PropertyGroup target, MapperContext context)
    {
        if (source.Id > 0)
        {
            target.Id = source.Id;
        }

        target.Key = source.Key;
        target.Type = source.Type;
        target.Name = source.Name;
        target.Alias = source.Alias;
        target.SortOrder = source.SortOrder;
    }

    // Umbraco.Code.MapAll -ContentTypeId -ParentTabContentTypes -ParentTabContentTypeNames
    private static void Map(
        PropertyGroupBasic<PropertyTypeBasic> source,
        PropertyGroupDisplay<PropertyTypeDisplay> target,
        MapperContext context)
    {
        target.Inherited = source.Inherited;
        if (source.Id > 0)
        {
            target.Id = source.Id;
        }

        target.Key = source.Key;
        target.Type = source.Type;
        target.Name = source.Name;
        target.Alias = source.Alias;
        target.SortOrder = source.SortOrder;
        target.Properties = context.MapEnumerable<PropertyTypeBasic, PropertyTypeDisplay>(source.Properties)
            .WhereNotNull();
    }

    // Umbraco.Code.MapAll -ContentTypeId -ParentTabContentTypes -ParentTabContentTypeNames
    private static void Map(
        PropertyGroupBasic<MemberPropertyTypeBasic> source,
        PropertyGroupDisplay<MemberPropertyTypeDisplay> target,
        MapperContext context)
    {
        target.Inherited = source.Inherited;
        if (source.Id > 0)
        {
            target.Id = source.Id;
        }

        target.Key = source.Key;
        target.Type = source.Type;
        target.Name = source.Name;
        target.Alias = source.Alias;
        target.SortOrder = source.SortOrder;
        target.Properties =
            context.MapEnumerable<MemberPropertyTypeBasic, MemberPropertyTypeDisplay>(source.Properties).WhereNotNull();
    }

    // Umbraco.Code.MapAll -Editor -View -Config -ContentTypeId -ContentTypeName -Locked -DataTypeIcon -DataTypeName
    private static void Map(PropertyTypeBasic source, PropertyTypeDisplay target, MapperContext context)
    {
        target.Alias = source.Alias;
        target.AllowCultureVariant = source.AllowCultureVariant;
        target.AllowSegmentVariant = source.AllowSegmentVariant;
        target.DataTypeId = source.DataTypeId;
        target.DataTypeKey = source.DataTypeKey;
        target.Description = source.Description;
        target.GroupId = source.GroupId;
        target.Id = source.Id;
        target.Inherited = source.Inherited;
        target.Label = source.Label;
        target.SortOrder = source.SortOrder;
        target.Validation = source.Validation;
        target.LabelOnTop = source.LabelOnTop;
    }

    // Umbraco.Code.MapAll -Editor -View -Config -ContentTypeId -ContentTypeName -Locked -DataTypeIcon -DataTypeName
    private static void Map(MemberPropertyTypeBasic source, MemberPropertyTypeDisplay target, MapperContext context)
    {
        target.Alias = source.Alias;
        target.AllowCultureVariant = source.AllowCultureVariant;
        target.AllowSegmentVariant = source.AllowSegmentVariant;
        target.DataTypeId = source.DataTypeId;
        target.DataTypeKey = source.DataTypeKey;
        target.Description = source.Description;
        target.GroupId = source.GroupId;
        target.Id = source.Id;
        target.Inherited = source.Inherited;
        target.IsSensitiveData = source.IsSensitiveData;
        target.Label = source.Label;
        target.MemberCanEditProperty = source.MemberCanEditProperty;
        target.MemberCanViewProperty = source.MemberCanViewProperty;
        target.SortOrder = source.SortOrder;
        target.Validation = source.Validation;
        target.LabelOnTop = source.LabelOnTop;
    }

    // Umbraco.Code.MapAll -CreatorId -Level -SortOrder -Variations
    // Umbraco.Code.MapAll -CreateDate -UpdateDate -DeleteDate
    // Umbraco.Code.MapAll -ContentTypeComposition (done by AfterMapSaveToType)
    private static void MapSaveToTypeBase<TSource, TSourcePropertyType>(
        TSource source,
        IContentTypeComposition target,
        MapperContext context)
        where TSource : ContentTypeSave<TSourcePropertyType>
        where TSourcePropertyType : PropertyTypeBasic
    {
        // TODO: not so clean really
        var isPublishing = target is IContentType;

        var id = Convert.ToInt32(source.Id);
        if (id > 0)
        {
            target.Id = id;
        }

        target.Alias = source.Alias;
        target.Description = source.Description;
        target.Icon = source.Icon;
        target.IsContainer = source.IsContainer;
        target.IsElement = source.IsElement;
        target.Key = source.Key;
        target.Name = source.Name;
        target.ParentId = source.ParentId;
        target.Path = source.Path;
        target.Thumbnail = source.Thumbnail;

        target.AllowedAsRoot = source.AllowAsRoot;

        var allowedContentTypesUnchanged = target.AllowedContentTypes?.Select(x => x.Id.Value)
            .SequenceEqual(source.AllowedContentTypes) ?? false;

        if (allowedContentTypesUnchanged is false)
        {
            target.AllowedContentTypes = source.AllowedContentTypes.Select((t, i) => new ContentTypeSort(t, i));
        }

        if (!(target is IMemberType))
        {
            target.SetVariesBy(ContentVariation.Culture, source.AllowCultureVariant);
            target.SetVariesBy(ContentVariation.Segment, source.AllowSegmentVariant);
        }

        // handle property groups and property types
        // note that ContentTypeSave has
        // - all groups, inherited and local; only *one* occurrence per group *name*
        // - potentially including the generic properties group
        // - all properties, inherited and local
        //
        // also, see PropertyTypeGroupResolver.ResolveCore:
        // - if a group is local *and* inherited, then Inherited is true
        //   and the identifier is the identifier of the *local* group
        //
        // IContentTypeComposition AddPropertyGroup, AddPropertyType methods do some
        // unique-alias-checking, etc that is *not* compatible with re-mapping everything
        // the way we do it here, so we should exclusively do it by
        // - managing a property group's PropertyTypes collection
        // - managing the content type's PropertyTypes collection (for generic properties)

        // handle actual groups (non-generic-properties)
        PropertyGroup[] destOrigGroups = target.PropertyGroups.ToArray(); // local groups
        IPropertyType[] destOrigProperties = target.PropertyTypes.ToArray(); // all properties, in groups or not
        var destGroups = new List<PropertyGroup>();
        PropertyGroupBasic<TSourcePropertyType>[] sourceGroups =
            source.Groups.Where(x => x.IsGenericProperties == false).ToArray();
        var sourceGroupParentAliases = sourceGroups.Select(x => x.GetParentAlias()).Distinct().ToArray();
        foreach (PropertyGroupBasic<TSourcePropertyType> sourceGroup in sourceGroups)
        {
            // get the dest group
            PropertyGroup? destGroup = MapSaveGroup(sourceGroup, destOrigGroups, context);

            // handle local properties
            IPropertyType[] destProperties = sourceGroup.Properties
                .Where(x => x.Inherited == false)
                .Select(x => MapSaveProperty(x, destOrigProperties, context))
                .WhereNotNull()
                .ToArray();

            // if the group has no local properties and is not used as parent, skip it, ie sort-of garbage-collect
            // local groups which would not have local properties anymore
            if (destProperties.Length == 0 && !sourceGroupParentAliases.Contains(sourceGroup.Alias))
            {
                continue;
            }

            // ensure no duplicate alias, then assign the group properties collection
            EnsureUniqueAliases(destProperties);

            if (destGroup is not null)
            {
                if (destGroup.PropertyTypes?.SupportsPublishing != isPublishing ||
                    destGroup.PropertyTypes.SequenceEqual(destProperties) is false)
                {
                    destGroup.PropertyTypes = new PropertyTypeCollection(isPublishing, destProperties);
                }

                destGroups.Add(destGroup);
            }
        }

        // ensure no duplicate name, then assign the groups collection
        EnsureUniqueAliases(destGroups);

        if (target.PropertyGroups.SequenceEqual(destGroups) is false)
        {
            target.PropertyGroups = new PropertyGroupCollection(destGroups);
        }

        // because the property groups collection was rebuilt, there is no need to remove
        // the old groups - they are just gone and will be cleared by the repository

        // handle non-grouped (ie generic) properties
        PropertyGroupBasic<TSourcePropertyType>? genericPropertiesGroup =
            source.Groups.FirstOrDefault(x => x.IsGenericProperties);
        if (genericPropertiesGroup != null)
        {
            // handle local properties
            IPropertyType[] destProperties = genericPropertiesGroup.Properties
                .Where(x => x.Inherited == false)
                .Select(x => MapSaveProperty(x, destOrigProperties, context))
                .WhereNotNull()
                .ToArray();

            // ensure no duplicate alias, then assign the generic properties collection
            EnsureUniqueAliases(destProperties);
            target.NoGroupPropertyTypes = new PropertyTypeCollection(isPublishing, destProperties);
        }

        // because all property collections were rebuilt, there is no need to remove
        // some old properties, they are just gone and will be cleared by the repository
    }

    // Umbraco.Code.MapAll -Blueprints -Errors -ListViewEditorName -Trashed
    private void MapTypeToDisplayBase(IContentTypeComposition source, ContentTypeCompositionDisplay target)
    {
        target.Alias = source.Alias;
        target.AllowAsRoot = source.AllowedAsRoot;
        target.CreateDate = source.CreateDate;
        target.Description = source.Description;
        target.Icon = source.Icon;
        target.IconFilePath = target.IconIsClass
            ? string.Empty
            : $"{_globalSettings.GetBackOfficePath(_hostingEnvironment).EnsureEndsWith("/")}images/umbraco/{source.Icon}";
        target.Id = source.Id;
        target.IsContainer = source.IsContainer;
        target.IsElement = source.IsElement;
        target.Key = source.Key;
        target.Name = source.Name;
        target.ParentId = source.ParentId;
        target.Path = source.Path;
        target.Thumbnail = source.Thumbnail;
        target.ThumbnailFilePath = target.ThumbnailIsClass
            ? string.Empty
            : _hostingEnvironment.ToAbsolute("~/umbraco/images/thumbnails/" + source.Thumbnail);
        target.Udi = MapContentTypeUdi(source);
        target.UpdateDate = source.UpdateDate;

        target.AllowedContentTypes = source.AllowedContentTypes?.OrderBy(c => c.SortOrder).Select(x => x.Id.Value);
        target.CompositeContentTypes = source.ContentTypeComposition.Select(x => x.Alias);
        target.LockedCompositeContentTypes = MapLockedCompositions(source);
        target.Variations = source.Variations;
    }

    // no MapAll - relies on the non-generic method
    private void MapTypeToDisplayBase<TTarget, TTargetPropertyType>(IContentTypeComposition source, TTarget target)
        where TTarget : ContentTypeCompositionDisplay<TTargetPropertyType>
        where TTargetPropertyType : PropertyTypeDisplay, new()
    {
        MapTypeToDisplayBase(source, target);

        var groupsMapper = new PropertyTypeGroupMapper<TTargetPropertyType>(
            _propertyEditors,
            _dataTypeService,
            _shortStringHelper,
            _loggerFactory.CreateLogger<PropertyTypeGroupMapper<TTargetPropertyType>>());
        target.Groups = groupsMapper.Map(source);
    }

    // Umbraco.Code.MapAll -CreateDate -UpdateDate -ListViewEditorName -Errors -LockedCompositeContentTypes
    private void MapTypeToDisplayBase(ContentTypeSave source, ContentTypeCompositionDisplay target)
    {
        target.Alias = source.Alias;
        target.AllowAsRoot = source.AllowAsRoot;
        target.AllowedContentTypes = source.AllowedContentTypes;
        target.Blueprints = source.Blueprints;
        target.CompositeContentTypes = source.CompositeContentTypes;
        target.Description = source.Description;
        target.Icon = source.Icon;
        target.IconFilePath = target.IconIsClass
            ? string.Empty
            : $"{_globalSettings.GetBackOfficePath(_hostingEnvironment).EnsureEndsWith("/")}images/umbraco/{source.Icon}";
        target.Id = source.Id;
        target.IsContainer = source.IsContainer;
        target.IsElement = source.IsElement;
        target.Key = source.Key;
        target.Name = source.Name;
        target.ParentId = source.ParentId;
        target.Path = source.Path;
        target.Thumbnail = source.Thumbnail;
        target.ThumbnailFilePath = target.ThumbnailIsClass
            ? string.Empty
            : _hostingEnvironment.ToAbsolute("~/umbraco/images/thumbnails/" + source.Thumbnail);
        target.Trashed = source.Trashed;
        target.Udi = source.Udi;
        target.Variations = source.Variations;
    }

    // no MapAll - relies on the non-generic method
    private void MapTypeToDisplayBase<TSource, TSourcePropertyType, TTarget, TTargetPropertyType>(
        TSource source,
        TTarget target,
        MapperContext context)
        where TSource : ContentTypeSave<TSourcePropertyType>
        where TSourcePropertyType : PropertyTypeBasic
        where TTarget : ContentTypeCompositionDisplay<TTargetPropertyType>
        where TTargetPropertyType : PropertyTypeDisplay
    {
        MapTypeToDisplayBase(source, target);

        target.Groups =
            context
                .MapEnumerable<PropertyGroupBasic<TSourcePropertyType>, PropertyGroupDisplay<TTargetPropertyType>>(
                    source.Groups).WhereNotNull();
    }

    private IEnumerable<string> MapLockedCompositions(IContentTypeComposition source)
    {
        // get ancestor ids from path of parent if not root
        if (source.ParentId == Constants.System.Root)
        {
            return Enumerable.Empty<string>();
        }

        IContentType? parent = _contentTypeService.Get(source.ParentId);
        if (parent == null)
        {
            return Enumerable.Empty<string>();
        }

        var aliases = new List<string>();
        IEnumerable<int>? ancestorIds = parent.Path?.Split(Constants.CharArrays.Comma)
            .Select(s => int.Parse(s, CultureInfo.InvariantCulture));

        // loop through all content types and return ordered aliases of ancestors
        IContentType[] allContentTypes = _contentTypeService.GetAll().ToArray();
        if (ancestorIds is not null)
        {
            foreach (var ancestorId in ancestorIds)
            {
                IContentType? ancestor = allContentTypes.FirstOrDefault(x => x.Id == ancestorId);
                if (ancestor is not null && ancestor.Alias is not null)
                {
                    aliases.Add(ancestor.Alias);
                }
            }
        }

        return aliases.OrderBy(x => x);
    }

    private static PropertyGroup? MapSaveGroup<TPropertyType>(
        PropertyGroupBasic<TPropertyType> sourceGroup,
        IEnumerable<PropertyGroup> destOrigGroups,
        MapperContext context)
        where TPropertyType : PropertyTypeBasic
    {
        PropertyGroup? destGroup;
        if (sourceGroup.Id > 0)
        {
            // update an existing group
            // ensure it is still there, then map/update
            destGroup = destOrigGroups.FirstOrDefault(x => x.Id == sourceGroup.Id);
            if (destGroup != null)
            {
                context.Map(sourceGroup, destGroup);
                return destGroup;
            }

            // force-clear the ID as it does not match anything
            sourceGroup.Id = 0;
        }

        // insert a new group, or update an existing group that has
        // been deleted in the meantime and we need to re-create
        // map/create
        destGroup = context.Map<PropertyGroup>(sourceGroup);
        return destGroup;
    }

    private static IPropertyType? MapSaveProperty(
        PropertyTypeBasic sourceProperty,
        IEnumerable<IPropertyType> destOrigProperties,
        MapperContext context)
    {
        IPropertyType? destProperty;
        if (sourceProperty.Id > 0)
        {
            // updating an existing property
            // ensure it is still there, then map/update
            destProperty = destOrigProperties.FirstOrDefault(x => x.Id == sourceProperty.Id);
            if (destProperty != null)
            {
                context.Map(sourceProperty, destProperty);
                return destProperty;
            }

            // force-clear the ID as it does not match anything
            sourceProperty.Id = 0;
        }

        // insert a new property, or update an existing property that has
        // been deleted in the meantime and we need to re-create
        // map/create
        destProperty = context.Map<IPropertyType>(sourceProperty);
        return destProperty;
    }

    private static void EnsureUniqueAliases(IEnumerable<IPropertyType> properties)
    {
        IPropertyType[] propertiesA = properties.ToArray();
        var distinctProperties = propertiesA
            .Select(x => x.Alias?.ToUpperInvariant())
            .Distinct()
            .Count();
        if (distinctProperties != propertiesA.Length)
        {
            throw new InvalidOperationException("Cannot map properties due to alias conflict.");
        }
    }

    private static void EnsureUniqueAliases(IEnumerable<PropertyGroup> groups)
    {
        PropertyGroup[] groupsA = groups.ToArray();
        var distinctProperties = groupsA
            .Select(x => x.Alias)
            .Distinct()
            .Count();
        if (distinctProperties != groupsA.Length)
        {
            throw new InvalidOperationException("Cannot map groups due to alias conflict.");
        }
    }

    private static void MapComposition(ContentTypeSave source, IContentTypeComposition target, Func<string, IContentTypeComposition?> getContentType)
    {
        var current = target.CompositionAliases().ToArray();
        IEnumerable<string> proposed = source.CompositeContentTypes;

        IEnumerable<string> remove = current.Where(x => !proposed.Contains(x));
        IEnumerable<string> add = proposed.Where(x => !current.Contains(x));

        foreach (var alias in remove)
        {
            target.RemoveContentType(alias);
        }

        foreach (var alias in add)
        {
            // TODO: Remove N+1 lookup
            IContentTypeComposition? contentType = getContentType(alias);
            if (contentType != null)
            {
                target.AddContentType(contentType);
            }
        }
    }
}
