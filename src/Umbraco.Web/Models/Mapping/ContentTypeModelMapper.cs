using System;
using System.Linq;
using System.Threading;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Models.ContentEditing;
using System.Collections.Generic;
using AutoMapper.Internal;
using Umbraco.Core.Services;
using Property = umbraco.NodeFactory.Property;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Defines mappings for content/media/members type mappings
    /// </summary>
    internal class ContentTypeModelMapper : MapperConfiguration
    {
        private readonly Lazy<PropertyEditorResolver> _propertyEditorResolver;

        //default ctor
        public ContentTypeModelMapper()
        {
            _propertyEditorResolver = new Lazy<PropertyEditorResolver>(() => PropertyEditorResolver.Current);
        }

        //ctor can be used for testing
        public ContentTypeModelMapper(Lazy<PropertyEditorResolver> propertyEditorResolver)
        {
            _propertyEditorResolver = propertyEditorResolver;            
        }

        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            
            config.CreateMap<PropertyTypeBasic, PropertyType>()
                .ConstructUsing(basic => new PropertyType(applicationContext.Services.DataTypeService.GetDataTypeDefinitionById(basic.DataTypeId)))
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
                .ForMember(type => type.HasIdentity, expression => expression.Ignore());

            config.CreateMap<DocumentTypeSave, IContentType>()
                //do the base mapping
                .MapBaseContentTypeSaveToEntity<DocumentTypeSave, PropertyTypeBasic, IContentType>(applicationContext)
                .ConstructUsing((source) => new ContentType(source.ParentId))
                .ForMember(source => source.AllowedTemplates, expression => expression.Ignore())
                .ForMember(dto => dto.DefaultTemplate, expression => expression.Ignore())
                .AfterMap((source, dest) =>
                {
                    dest.AllowedTemplates = source.AllowedTemplates
                        .Where(x => x != null)
                        .Select(s => applicationContext.Services.FileService.GetTemplate(s))
                        .ToArray();

                    if (source.DefaultTemplate != null)
                        dest.SetDefaultTemplate(applicationContext.Services.FileService.GetTemplate(source.DefaultTemplate));

                    ContentTypeModelMapperExtensions.AfterMapContentTypeSaveToEntity(source, dest, applicationContext);
                });

            config.CreateMap<MediaTypeSave, IMediaType>()
                //do the base mapping
                .MapBaseContentTypeSaveToEntity<MediaTypeSave, PropertyTypeBasic, IMediaType>(applicationContext)
                .ConstructUsing((source) => new MediaType(source.ParentId))                
                .AfterMap((source, dest) =>
                {
                    ContentTypeModelMapperExtensions.AfterMapMediaTypeSaveToEntity(source, dest, applicationContext);
                });

            config.CreateMap<MemberTypeSave, IMemberType>()
                //do the base mapping
                .MapBaseContentTypeSaveToEntity<MemberTypeSave, MemberPropertyTypeBasic, IMemberType>(applicationContext)
                .ConstructUsing((source) => new MemberType(source.ParentId))
                .AfterMap((source, dest) =>
                {
                    ContentTypeModelMapperExtensions.AfterMapContentTypeSaveToEntity(source, dest, applicationContext);

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
                .MapBaseContentTypeEntityToDisplay<IMemberType, MemberTypeDisplay, MemberPropertyTypeDisplay>(applicationContext, _propertyEditorResolver)
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
                .MapBaseContentTypeEntityToDisplay<IMediaType, MediaTypeDisplay, PropertyTypeDisplay>(applicationContext, _propertyEditorResolver)
                .AfterMap((source, dest) =>
                 {
                     //default listview
                     dest.ListViewEditorName = Constants.Conventions.DataTypes.ListViewPrefix + "Media";

                     if (string.IsNullOrEmpty(source.Name) == false)
                     {
                         var name = Constants.Conventions.DataTypes.ListViewPrefix + source.Name;
                         if (applicationContext.Services.DataTypeService.GetDataTypeDefinitionByName(name) != null)
                             dest.ListViewEditorName = name;
                     }
                 });

            config.CreateMap<IContentType, DocumentTypeDisplay>()
                //map base logic
                .MapBaseContentTypeEntityToDisplay<IContentType, DocumentTypeDisplay, PropertyTypeDisplay>(applicationContext, _propertyEditorResolver)
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
                        if (applicationContext.Services.DataTypeService.GetDataTypeDefinitionByName(name) != null)
                            dest.ListViewEditorName = name;
                    }

                });

            config.CreateMap<IMemberType, ContentTypeBasic>();
            config.CreateMap<IMediaType, ContentTypeBasic>();
            config.CreateMap<IContentType, ContentTypeBasic>();

            config.CreateMap<PropertyTypeBasic, PropertyType>()

                .ConstructUsing(propertyTypeBasic =>
                {
                    var dataType = applicationContext.Services.DataTypeService.GetDataTypeDefinitionById(propertyTypeBasic.DataTypeId);
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
                        var templates = applicationContext.Services.FileService.GetTemplates(source.AllowedTemplates.ToArray());
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
                            var template = applicationContext.Services.FileService.GetTemplate(source.DefaultTemplate);
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