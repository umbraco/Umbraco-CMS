using System;
using System.Linq;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Core.Services;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Defines mappings for content/media/members type mappings
    /// </summary>
    internal class ContentTypeMapperProfile : Profile
    {
        private readonly PropertyEditorCollection _propertyEditors;
        private readonly IDataTypeService _dataTypeService;
        private readonly IFileService _fileService;
        private readonly IContentTypeService _contentTypeService;
        private readonly IMediaTypeService _mediaTypeService;

        public ContentTypeMapperProfile(PropertyEditorCollection propertyEditors, IDataTypeService dataTypeService, IFileService fileService, IContentTypeService contentTypeService, IMediaTypeService mediaTypeService)
        {
            _propertyEditors = propertyEditors;
            _dataTypeService = dataTypeService;
            _fileService = fileService;
            _contentTypeService = contentTypeService;
            _mediaTypeService = mediaTypeService;

            // v7 creates this map twice which makes no sense, and AutoMapper 6 detects it
            // assuming the second map took over, and removing the first one for now
            /*
            CreateMap<PropertyTypeBasic, PropertyType>()
                .ConstructUsing(basic => new PropertyType(_dataTypeService.GetDataTypeDefinitionById(basic.DataTypeId)))
                .ForMember(type => type.ValidationRegExp, opt => opt.ResolveUsing(basic => basic.Validation.Pattern))
                .ForMember(type => type.Mandatory, opt => opt.ResolveUsing(basic => basic.Validation.Mandatory))
                .ForMember(type => type.Name, opt => opt.ResolveUsing(basic => basic.Label))
                .ForMember(type => type.DataTypeDefinitionId, opt => opt.ResolveUsing(basic => basic.DataTypeId))
                .ForMember(type => type.DataTypeId, opt => opt.Ignore())
                .ForMember(type => type.PropertyEditorAlias, opt => opt.Ignore())
                .ForMember(type => type.HelpText, opt => opt.Ignore())
                .ForMember(type => type.Key, opt => opt.Ignore())
                .ForMember(type => type.CreateDate, opt => opt.Ignore())
                .ForMember(type => type.UpdateDate, opt => opt.Ignore())
                .ForMember(type => type.DeletedDate, opt => opt.Ignore())
                .ForMember(type => type.HasIdentity, opt => opt.Ignore());
            */

            CreateMap<DocumentTypeSave, IContentType>()
                //do the base mapping
                .MapBaseContentTypeSaveToEntity<DocumentTypeSave, PropertyTypeBasic, IContentType>()
                .ConstructUsing((source) => new ContentType(source.ParentId))
                .ForMember(source => source.AllowedTemplates, opt => opt.Ignore())
                .ForMember(dto => dto.DefaultTemplate, opt => opt.Ignore())
                .AfterMap((source, dest) =>
                {
                    dest.AllowedTemplates = source.AllowedTemplates
                        .Where(x => x != null)
                        .Select(s => _fileService.GetTemplate(s))
                        .ToArray();

                    if (source.DefaultTemplate != null)
                        dest.SetDefaultTemplate(_fileService.GetTemplate(source.DefaultTemplate));

                    ContentTypeProfileExtensions.AfterMapContentTypeSaveToEntity(source, dest, _contentTypeService);
                });

            CreateMap<MediaTypeSave, IMediaType>()
                //do the base mapping
                .MapBaseContentTypeSaveToEntity<MediaTypeSave, PropertyTypeBasic, IMediaType>()
                .ConstructUsing((source) => new MediaType(source.ParentId))
                .AfterMap((source, dest) =>
                {
                    ContentTypeProfileExtensions.AfterMapMediaTypeSaveToEntity(source, dest, _mediaTypeService);
                });

            CreateMap<MemberTypeSave, IMemberType>()
                //do the base mapping
                .MapBaseContentTypeSaveToEntity<MemberTypeSave, MemberPropertyTypeBasic, IMemberType>()
                .ConstructUsing(source => new MemberType(source.ParentId))
                .AfterMap((source, dest) =>
                {
                    ContentTypeProfileExtensions.AfterMapContentTypeSaveToEntity(source, dest, _contentTypeService);

                    //map the MemberCanEditProperty,MemberCanViewProperty
                    foreach (var propertyType in source.Groups.SelectMany(x => x.Properties))
                    {
                        var localCopy = propertyType;
                        var destProp = dest.PropertyTypes.SingleOrDefault(x => x.Alias.InvariantEquals(localCopy.Alias));
                        if (destProp != null)
                        {
                            dest.SetMemberCanEditProperty(localCopy.Alias, localCopy.MemberCanEditProperty);
                            dest.SetMemberCanViewProperty(localCopy.Alias, localCopy.MemberCanViewProperty);
                        }
                    }
                });

            CreateMap<IContentTypeComposition, string>().ConvertUsing(dest => dest.Alias);

            CreateMap<IMemberType, MemberTypeDisplay>()
                //map base logic
                .MapBaseContentTypeEntityToDisplay<IMemberType, MemberTypeDisplay, MemberPropertyTypeDisplay>(_propertyEditors, _dataTypeService, _contentTypeService)
                .AfterMap((memberType, display) =>
                {
                    //map the MemberCanEditProperty,MemberCanViewProperty
                    foreach (var propertyType in memberType.PropertyTypes)
                    {
                        var localCopy = propertyType;
                        var displayProp = display.Groups.SelectMany(dest => dest.Properties).SingleOrDefault(dest => dest.Alias.InvariantEquals(localCopy.Alias));
                        if (displayProp != null)
                        {
                            displayProp.MemberCanEditProperty = memberType.MemberCanEditProperty(localCopy.Alias);
                            displayProp.MemberCanViewProperty = memberType.MemberCanViewProperty(localCopy.Alias);
                        }
                    }
                });

            CreateMap<IMediaType, MediaTypeDisplay>()
                //map base logic
                .MapBaseContentTypeEntityToDisplay<IMediaType, MediaTypeDisplay, PropertyTypeDisplay>(_propertyEditors, _dataTypeService, _contentTypeService)
                .AfterMap((source, dest) =>
                 {
                     //default listview
                     dest.ListViewEditorName = Constants.Conventions.DataTypes.ListViewPrefix + "Media";

                     if (string.IsNullOrEmpty(source.Name) == false)
                     {
                         var name = Constants.Conventions.DataTypes.ListViewPrefix + source.Name;
                         if (_dataTypeService.GetDataType(name) != null)
                             dest.ListViewEditorName = name;
                     }
                 });

            CreateMap<IContentType, DocumentTypeDisplay>()
                //map base logic
                .MapBaseContentTypeEntityToDisplay<IContentType, DocumentTypeDisplay, PropertyTypeDisplay>(_propertyEditors, _dataTypeService, _contentTypeService)
                .ForMember(dto => dto.AllowedTemplates, opt => opt.Ignore())
                .ForMember(dto => dto.DefaultTemplate, opt => opt.Ignore())
                .ForMember(display => display.Notifications, opt => opt.Ignore())
                .AfterMap((source, dest) =>
                {
                    //sync templates
                    dest.AllowedTemplates = source.AllowedTemplates.Select(Mapper.Map<EntityBasic>).ToArray();

                    if (source.DefaultTemplate != null)
                        dest.DefaultTemplate = Mapper.Map<EntityBasic>(source.DefaultTemplate);

                    //default listview
                    dest.ListViewEditorName = Constants.Conventions.DataTypes.ListViewPrefix + "Content";

                    if (string.IsNullOrEmpty(source.Alias) == false)
                    {
                        var name = Constants.Conventions.DataTypes.ListViewPrefix + source.Alias;
                        if (_dataTypeService.GetDataType(name) != null)
                            dest.ListViewEditorName = name;
                    }

                });

            CreateMap<IMemberType, ContentTypeBasic>()
                .ForMember(dest => dest.Udi, opt => opt.MapFrom(source => Udi.Create(Constants.UdiEntityType.MemberType, source.Key)))
                .ForMember(dest => dest.Blueprints, opt => opt.Ignore());
            CreateMap<IMediaType, ContentTypeBasic>()
                .ForMember(dest => dest.Udi, opt => opt.MapFrom(source => Udi.Create(Constants.UdiEntityType.MediaType, source.Key)))
                .ForMember(dest => dest.Blueprints, opt => opt.Ignore());
            CreateMap<IContentType, ContentTypeBasic>()
                .ForMember(dest => dest.Udi, opt => opt.MapFrom(source => Udi.Create(Constants.UdiEntityType.DocumentType, source.Key)))
                .ForMember(dest => dest.Blueprints, opt => opt.Ignore());

            CreateMap<PropertyTypeBasic, PropertyType>()

                .ConstructUsing(propertyTypeBasic =>
                {
                    var dataType = _dataTypeService.GetDataType(propertyTypeBasic.DataTypeId);
                    if (dataType == null) throw new NullReferenceException("No data type found with id " + propertyTypeBasic.DataTypeId);
                    return new PropertyType(dataType, propertyTypeBasic.Alias);
                })

                .IgnoreEntityCommonProperties()

                .ForMember(dest => dest.IsPublishing, opt => opt.Ignore())

                // see note above - have to do this here?
                .ForMember(dest => dest.PropertyEditorAlias, opt => opt.Ignore())
                .ForMember(dest => dest.DeleteDate, opt => opt.Ignore())

                .ForMember(dto => dto.Variations, opt => opt.Ignore()) // fixme - change when UI supports it!

                //only map if it is actually set
                .ForMember(dest => dest.Id, opt => opt.Condition(source => source.Id > 0))
                //only map if it is actually set, if it's  not set, it needs to be handled differently and will be taken care of in the
                // IContentType.AddPropertyType
                .ForMember(dest => dest.PropertyGroupId, opt => opt.Condition(source => source.GroupId > 0))
                .ForMember(dest => dest.PropertyGroupId, opt => opt.MapFrom(display => new Lazy<int>(() => display.GroupId, false)))
                .ForMember(dest => dest.Key, opt => opt.Ignore())
                .ForMember(dest => dest.HelpText, opt => opt.Ignore())
                .ForMember(dest => dest.HasIdentity, opt => opt.Ignore())
                //ignore because this is set in the ctor NOT ON UPDATE, STUPID!
                //.ForMember(type => type.Alias, opt => opt.Ignore())
                //ignore because this is obsolete and shouldn't be used
                .ForMember(dest => dest.DataTypeId, opt => opt.Ignore())
                .ForMember(dest => dest.Mandatory, opt => opt.MapFrom(source => source.Validation.Mandatory))
                .ForMember(dest => dest.ValidationRegExp, opt => opt.MapFrom(source => source.Validation.Pattern))
                .ForMember(dest => dest.DataTypeDefinitionId, opt => opt.MapFrom(source => source.DataTypeId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(source => source.Label));

            #region *** Used for mapping on top of an existing display object from a save object ***

            CreateMap<MemberTypeSave, MemberTypeDisplay>()
                .MapBaseContentTypeSaveToDisplay<MemberTypeSave, MemberPropertyTypeBasic, MemberTypeDisplay, MemberPropertyTypeDisplay>();

            CreateMap<MediaTypeSave, MediaTypeDisplay>()
                .MapBaseContentTypeSaveToDisplay<MediaTypeSave, PropertyTypeBasic, MediaTypeDisplay, PropertyTypeDisplay>();

            CreateMap<DocumentTypeSave, DocumentTypeDisplay>()
                .MapBaseContentTypeSaveToDisplay<DocumentTypeSave, PropertyTypeBasic, DocumentTypeDisplay, PropertyTypeDisplay>()
                .ForMember(dto => dto.AllowedTemplates, opt => opt.Ignore())
                .ForMember(dto => dto.DefaultTemplate, opt => opt.Ignore())
                .AfterMap((source, dest) =>
                {
                    //sync templates
                    var destAllowedTemplateAliases = dest.AllowedTemplates.Select(x => x.Alias);
                    //if the dest is set and it's the same as the source, then don't change
                    if (destAllowedTemplateAliases.SequenceEqual(source.AllowedTemplates) == false)
                    {
                        var templates = _fileService.GetTemplates(source.AllowedTemplates.ToArray());
                        dest.AllowedTemplates = source.AllowedTemplates
                            .Select(x => Mapper.Map<EntityBasic>(templates.SingleOrDefault(t => t.Alias == x)))
                            .WhereNotNull()
                            .ToArray();
                    }

                    if (source.DefaultTemplate.IsNullOrWhiteSpace() == false)
                    {
                        //if the dest is set and it's the same as the source, then don't change
                        if (dest.DefaultTemplate == null || source.DefaultTemplate != dest.DefaultTemplate.Alias)
                        {
                            var template = _fileService.GetTemplate(source.DefaultTemplate);
                            dest.DefaultTemplate = template == null ? null : Mapper.Map<EntityBasic>(template);
                        }
                    }
                    else
                    {
                        dest.DefaultTemplate = null;
                    }
                });

            //for doc types, media types
            CreateMap<PropertyGroupBasic<PropertyTypeBasic>, PropertyGroup>()
                .MapPropertyGroupBasicToPropertyGroupPersistence<PropertyGroupBasic<PropertyTypeBasic>, PropertyTypeBasic>();

            //for members
            CreateMap<PropertyGroupBasic<MemberPropertyTypeBasic>, PropertyGroup>()
                .MapPropertyGroupBasicToPropertyGroupPersistence<PropertyGroupBasic<MemberPropertyTypeBasic>, MemberPropertyTypeBasic>();

            //for doc types, media types
            CreateMap<PropertyGroupBasic<PropertyTypeBasic>, PropertyGroupDisplay<PropertyTypeDisplay>>()
                .MapPropertyGroupBasicToPropertyGroupDisplay<PropertyGroupBasic<PropertyTypeBasic>, PropertyTypeBasic, PropertyTypeDisplay>();

            //for members
            CreateMap<PropertyGroupBasic<MemberPropertyTypeBasic>, PropertyGroupDisplay<MemberPropertyTypeDisplay>>()
                .MapPropertyGroupBasicToPropertyGroupDisplay<PropertyGroupBasic<MemberPropertyTypeBasic>, MemberPropertyTypeBasic, MemberPropertyTypeDisplay>();

            CreateMap<PropertyTypeBasic, PropertyTypeDisplay>()
                .ForMember(g => g.Editor, opt => opt.Ignore())
                .ForMember(g => g.View, opt => opt.Ignore())
                .ForMember(g => g.Config, opt => opt.Ignore())
                .ForMember(g => g.ContentTypeId, opt => opt.Ignore())
                .ForMember(g => g.ContentTypeName, opt => opt.Ignore())
                .ForMember(g => g.Locked, exp => exp.Ignore());

            CreateMap<MemberPropertyTypeBasic, MemberPropertyTypeDisplay>()
                .ForMember(g => g.Editor, opt => opt.Ignore())
                .ForMember(g => g.View, opt => opt.Ignore())
                .ForMember(g => g.Config, opt => opt.Ignore())
                .ForMember(g => g.ContentTypeId, opt => opt.Ignore())
                .ForMember(g => g.ContentTypeName, opt => opt.Ignore())
                .ForMember(g => g.Locked, exp => exp.Ignore());

            #endregion

        }
    }
}
