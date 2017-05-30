using System;
using System.Linq;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Core.Services;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Defines mappings for content/media/members type mappings
    /// </summary>
    internal class ContentTypeModelMapper : ModelMapperConfiguration
    {
        private readonly PropertyEditorCollection _propertyEditors;
        private readonly IDataTypeService _dataTypeService;
        private readonly IFileService _fileService;
        private readonly IContentTypeService _contentTypeService;
        private readonly IMediaTypeService _mediaTypeService;

        public ContentTypeModelMapper(PropertyEditorCollection propertyEditors, IDataTypeService dataTypeService, IFileService fileService, IContentTypeService contentTypeService, IMediaTypeService mediaTypeService)
        {
            _propertyEditors = propertyEditors;
            _dataTypeService = dataTypeService;
            _fileService = fileService;
            _contentTypeService = contentTypeService;
            _mediaTypeService = mediaTypeService;
        }

        public override void ConfigureMappings(IMapperConfiguration config)
        {
            
            config.CreateMap<PropertyTypeBasic, PropertyType>()
                .ConstructUsing(basic => new PropertyType(_dataTypeService.GetDataTypeDefinitionById(basic.DataTypeId)))
                .ForMember(type => type.ValidationRegExp, expression => expression.ResolveUsing(basic => basic.Validation.Pattern))
                .ForMember(type => type.Mandatory, expression => expression.ResolveUsing(basic => basic.Validation.Mandatory))
                .ForMember(type => type.Name, expression => expression.ResolveUsing(basic => basic.Label))
                .ForMember(type => type.DataTypeDefinitionId, expression => expression.ResolveUsing(basic => basic.DataTypeId))
                .ForMember(type => type.DataTypeId, expression => expression.Ignore())
                .ForMember(type => type.PropertyEditorAlias, expression => expression.Ignore())
                .ForMember(type => type.HelpText, expression => expression.Ignore())
                .ForMember(type => type.Key, expression => expression.Ignore())
                .ForMember(type => type.CreateDate, expression => expression.Ignore())
                .ForMember(type => type.UpdateDate, expression => expression.Ignore())
                .ForMember(type => type.DeletedDate, expression => expression.Ignore())
                .ForMember(type => type.HasIdentity, expression => expression.Ignore());

            config.CreateMap<DocumentTypeSave, IContentType>()
                //do the base mapping
                .MapBaseContentTypeSaveToEntity<DocumentTypeSave, PropertyTypeBasic, IContentType>()
                .ConstructUsing((source) => new ContentType(source.ParentId))
                .ForMember(source => source.AllowedTemplates, expression => expression.Ignore())
                .ForMember(dto => dto.DefaultTemplate, expression => expression.Ignore())
                .AfterMap((source, dest) =>
                {
                    dest.AllowedTemplates = source.AllowedTemplates
                        .Where(x => x != null)
                        .Select(s => _fileService.GetTemplate(s))
                        .ToArray();

                    if (source.DefaultTemplate != null)
                        dest.SetDefaultTemplate(_fileService.GetTemplate(source.DefaultTemplate));

                    ContentTypeModelMapperExtensions.AfterMapContentTypeSaveToEntity(source, dest, _contentTypeService);
                });

            config.CreateMap<MediaTypeSave, IMediaType>()
                //do the base mapping
                .MapBaseContentTypeSaveToEntity<MediaTypeSave, PropertyTypeBasic, IMediaType>()
                .ConstructUsing((source) => new MediaType(source.ParentId))
                .AfterMap((source, dest) =>
                {
                    ContentTypeModelMapperExtensions.AfterMapMediaTypeSaveToEntity(source, dest, _mediaTypeService);
                });

            config.CreateMap<MemberTypeSave, IMemberType>()
                //do the base mapping
                .MapBaseContentTypeSaveToEntity<MemberTypeSave, MemberPropertyTypeBasic, IMemberType>()
                .ConstructUsing((source) => new MemberType(source.ParentId))
                .AfterMap((source, dest) =>
                {
                    ContentTypeModelMapperExtensions.AfterMapContentTypeSaveToEntity(source, dest, _contentTypeService);

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

            config.CreateMap<IContentTypeComposition, string>().ConvertUsing(x => x.Alias);

            config.CreateMap<IMemberType, MemberTypeDisplay>()
                //map base logic
                .MapBaseContentTypeEntityToDisplay<IMemberType, MemberTypeDisplay, MemberPropertyTypeDisplay>(_propertyEditors, _dataTypeService, _contentTypeService)
                .AfterMap((memberType, display) =>
                {
                    //map the MemberCanEditProperty,MemberCanViewProperty
                    foreach (var propertyType in memberType.PropertyTypes)
                    {
                        var localCopy = propertyType;
                        var displayProp = display.Groups.SelectMany(x => x.Properties).SingleOrDefault(x => x.Alias.InvariantEquals(localCopy.Alias));
                        if (displayProp != null)
                        {
                            displayProp.MemberCanEditProperty = memberType.MemberCanEditProperty(localCopy.Alias);
                            displayProp.MemberCanViewProperty = memberType.MemberCanViewProperty(localCopy.Alias);
                        }
                    }
                });

            config.CreateMap<IMediaType, MediaTypeDisplay>()
                //map base logic
                .MapBaseContentTypeEntityToDisplay<IMediaType, MediaTypeDisplay, PropertyTypeDisplay>(_propertyEditors, _dataTypeService, _contentTypeService)
                .AfterMap((source, dest) =>
                 {
                     //default listview
                     dest.ListViewEditorName = Constants.Conventions.DataTypes.ListViewPrefix + "Media";

                     if (string.IsNullOrEmpty(source.Name) == false)
                     {
                         var name = Constants.Conventions.DataTypes.ListViewPrefix + source.Name;
                         if (_dataTypeService.GetDataTypeDefinitionByName(name) != null)
                             dest.ListViewEditorName = name;
                     }
                 });

            config.CreateMap<IContentType, DocumentTypeDisplay>()
                //map base logic
                .MapBaseContentTypeEntityToDisplay<IContentType, DocumentTypeDisplay, PropertyTypeDisplay>(_propertyEditors, _dataTypeService, _contentTypeService)
                .ForMember(dto => dto.AllowedTemplates, expression => expression.Ignore())
                .ForMember(dto => dto.DefaultTemplate, expression => expression.Ignore())
                .ForMember(display => display.Notifications, expression => expression.Ignore())
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
                        if (_dataTypeService.GetDataTypeDefinitionByName(name) != null)
                            dest.ListViewEditorName = name;
                    }

                });

            config.CreateMap<IMemberType, ContentTypeBasic>()
                .ForMember(x => x.Udi, expression => expression.MapFrom(content => Udi.Create(Constants.UdiEntityType.MemberType, content.Key)));
            config.CreateMap<IMediaType, ContentTypeBasic>()
                .ForMember(x => x.Udi, expression => expression.MapFrom(content => Udi.Create(Constants.UdiEntityType.MediaType, content.Key)));
            config.CreateMap<IContentType, ContentTypeBasic>()
                .ForMember(x => x.Udi, expression => expression.MapFrom(content => Udi.Create(Constants.UdiEntityType.DocumentType, content.Key)));

            config.CreateMap<PropertyTypeBasic, PropertyType>()

                .ConstructUsing(propertyTypeBasic =>
                {
                    var dataType = _dataTypeService.GetDataTypeDefinitionById(propertyTypeBasic.DataTypeId);
                    if (dataType == null) throw new NullReferenceException("No data type found with id " + propertyTypeBasic.DataTypeId);
                    return new PropertyType(dataType, propertyTypeBasic.Alias);
                })

                //only map if it is actually set
                .ForMember(dest => dest.Id, expression => expression.Condition(source => source.Id > 0))
                .ForMember(dto => dto.CreateDate, expression => expression.Ignore())
                .ForMember(dto => dto.UpdateDate, expression => expression.Ignore())
                //only map if it is actually set, if it's  not set, it needs to be handled differently and will be taken care of in the 
                // IContentType.AddPropertyType
                .ForMember(dest => dest.PropertyGroupId, expression => expression.Condition(source => source.GroupId > 0))
                .ForMember(type => type.PropertyGroupId, expression => expression.MapFrom(display => new Lazy<int>(() => display.GroupId, false)))
                .ForMember(type => type.Key, expression => expression.Ignore())
                .ForMember(type => type.HelpText, expression => expression.Ignore())
                .ForMember(type => type.HasIdentity, expression => expression.Ignore())
                //ignore because this is set in the ctor NOT ON UPDATE, STUPID!
                //.ForMember(type => type.Alias, expression => expression.Ignore())
                //ignore because this is obsolete and shouldn't be used
                .ForMember(type => type.DataTypeId, expression => expression.Ignore())
                .ForMember(type => type.Mandatory, expression => expression.MapFrom(display => display.Validation.Mandatory))
                .ForMember(type => type.ValidationRegExp, expression => expression.MapFrom(display => display.Validation.Pattern))
                .ForMember(type => type.DataTypeDefinitionId, expression => expression.MapFrom(display => display.DataTypeId))
                .ForMember(type => type.Name, expression => expression.MapFrom(display => display.Label));

            #region *** Used for mapping on top of an existing display object from a save object ***

            config.CreateMap<MemberTypeSave, MemberTypeDisplay>()
                .MapBaseContentTypeSaveToDisplay<MemberTypeSave, MemberPropertyTypeBasic, MemberTypeDisplay, MemberPropertyTypeDisplay>();

            config.CreateMap<MediaTypeSave, MediaTypeDisplay>()
                .MapBaseContentTypeSaveToDisplay<MediaTypeSave, PropertyTypeBasic, MediaTypeDisplay, PropertyTypeDisplay>();
            
            config.CreateMap<DocumentTypeSave, DocumentTypeDisplay>()
                .MapBaseContentTypeSaveToDisplay<DocumentTypeSave, PropertyTypeBasic, DocumentTypeDisplay, PropertyTypeDisplay>()
                .ForMember(dto => dto.AllowedTemplates, expression => expression.Ignore())
                .ForMember(dto => dto.DefaultTemplate, expression => expression.Ignore())
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
            config.CreateMap<PropertyGroupBasic<PropertyTypeBasic>, PropertyGroup>()
                .MapPropertyGroupBasicToPropertyGroupPersistence<PropertyGroupBasic<PropertyTypeBasic>, PropertyTypeBasic>();

            //for members
            config.CreateMap<PropertyGroupBasic<MemberPropertyTypeBasic>, PropertyGroup>()
                .MapPropertyGroupBasicToPropertyGroupPersistence<PropertyGroupBasic<MemberPropertyTypeBasic>, MemberPropertyTypeBasic>();

            //for doc types, media types
            config.CreateMap<PropertyGroupBasic<PropertyTypeBasic>, PropertyGroupDisplay<PropertyTypeDisplay>>()
                .MapPropertyGroupBasicToPropertyGroupDisplay<PropertyGroupBasic<PropertyTypeBasic>, PropertyTypeBasic, PropertyTypeDisplay>();

            //for members
            config.CreateMap<PropertyGroupBasic<MemberPropertyTypeBasic>, PropertyGroupDisplay<MemberPropertyTypeDisplay>>()
                .MapPropertyGroupBasicToPropertyGroupDisplay<PropertyGroupBasic<MemberPropertyTypeBasic>, MemberPropertyTypeBasic, MemberPropertyTypeDisplay>();

            config.CreateMap<PropertyTypeBasic, PropertyTypeDisplay>()
                .ForMember(g => g.Editor, expression => expression.Ignore())
                .ForMember(g => g.View, expression => expression.Ignore())
                .ForMember(g => g.Config, expression => expression.Ignore())
                .ForMember(g => g.ContentTypeId, expression => expression.Ignore())
                .ForMember(g => g.ContentTypeName, expression => expression.Ignore())
                .ForMember(g => g.Locked, exp => exp.Ignore());

            config.CreateMap<MemberPropertyTypeBasic, MemberPropertyTypeDisplay>()
                .ForMember(g => g.Editor, expression => expression.Ignore())
                .ForMember(g => g.View, expression => expression.Ignore())
                .ForMember(g => g.Config, expression => expression.Ignore())
                .ForMember(g => g.ContentTypeId, expression => expression.Ignore())
                .ForMember(g => g.ContentTypeName, expression => expression.Ignore())
                .ForMember(g => g.Locked, exp => exp.Ignore());

            #endregion




        }


    }
}