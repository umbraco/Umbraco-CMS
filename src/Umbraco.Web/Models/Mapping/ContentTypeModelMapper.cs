using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Models.ContentEditing;
using System.Collections.Generic;

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
            config.CreateMap<IMediaType, ContentTypeBasic>();
            config.CreateMap<IContentType, ContentTypeBasic>();
          

            config.CreateMap<ContentTypeDisplay, IContentType>()
                .ConstructUsing((ContentTypeDisplay source) => new ContentType(source.ParentId))

                //only map id if set to something higher then zero
                .ForMember(dto => dto.Id, expression => expression.Condition(display => (Convert.ToInt32(display.Id) > 0)))
                .ForMember(dto => dto.Id, expression => expression.MapFrom(display => Convert.ToInt32(display.Id)))
                .ForMember(dto => dto.AllowedAsRoot, expression => expression.MapFrom(display => display.AllowAsRoot))
                .ForMember(dto => dto.CreatorId, expression => expression.Ignore())
                .ForMember(dto => dto.Level, expression => expression.Ignore())
                .ForMember(dto => dto.SortOrder, expression => expression.Ignore())
                //mapped in aftermap
                .ForMember(dto => dto.AllowedContentTypes, expression => expression.Ignore())

                //ignore, we'll do this in after map
                .ForMember(dto => dto.PropertyGroups, expression => expression.Ignore())
                .AfterMap((source, dest) =>
                {
                    dest.PropertyGroups = new PropertyGroupCollection();
                    foreach (var groupDisplay in source.Groups.Where(x => x.Name.IsNullOrWhiteSpace() == false ) )
                    {
                        dest.PropertyGroups.Add(Mapper.Map<PropertyGroup>(groupDisplay));
                    }

                    //Sync allowed child types
                    var allowedTypes = source.AllowedContentTypes.Select((t, i) => new ContentTypeSort(t, i));

                    dest.AllowedContentTypes = allowedTypes.ToArray();

                    //sync compositions
                    var current = dest.CompositionAliases().ToArray();
                    var proposed = source.CompositeContentTypes;

                    var remove = current.Where(x =>  proposed.Contains(x) == false);
                    var add = proposed.Where(x => current.Contains(x) == false);

                    foreach (var rem in remove)
                    {
                        dest.RemoveContentType(rem);
                    }

                    foreach(var a in add)
                    {
                        //TODO: Remove N+1 lookup
                        var addCt = applicationContext.Services.ContentTypeService.GetContentType(a);
                        if(addCt != null)
                             dest.AddContentType(addCt);
                    }
                });


            config.CreateMap<ContentTypeSort, int>().ConvertUsing(x => x.Id.Value);
            config.CreateMap<IContentTypeComposition, string>().ConvertUsing(x => x.Alias);

            config.CreateMap<IContentType, ContentTypeDisplay>()
                .ForMember(display => display.AllowAsRoot, expression => expression.MapFrom(type => type.AllowedAsRoot))
                //Ignore because this is not actually used for content types
                .ForMember(display => display.Trashed, expression => expression.Ignore())

                .ForMember(
                    dto => dto.AllowedContentTypes,
                    expression => expression.MapFrom(dto => dto.AllowedContentTypes.Select(x => x.Id.Value)))

                .ForMember(
                    dto => dto.AvailableCompositeContentTypes,
                    expression => expression.ResolveUsing(new AvailableCompositeContentTypesResolver(applicationContext)))

                .ForMember(
                    dto => dto.CompositeContentTypes,
                    expression => expression.MapFrom(dto => dto.ContentTypeComposition))
                    
                .ForMember(
                    dto => dto.Groups,
                    expression => expression.ResolveUsing(new PropertyTypeGroupResolver(applicationContext, _propertyEditorResolver)));

            config.CreateMap<PropertyGroupDisplay, PropertyGroup>()
                .ForMember(dest => dest.Id, expression => expression.Condition(source => source.Id > 0))
                .ForMember(g => g.Key, expression => expression.Ignore())
                .ForMember(g => g.HasIdentity, expression => expression.Ignore())
                .ForMember(dto => dto.CreateDate, expression => expression.Ignore())
                .ForMember(dto => dto.UpdateDate, expression => expression.Ignore())

                //only map if a parent is actually set
                .ForMember(g => g.ParentId, expression => expression.Condition(display => display.ParentGroupId > 0))
                .ForMember(g => g.ParentId, expression => expression.MapFrom(display => display.ParentGroupId))
                
                //ignore these, we'll do this in after map
                .ForMember(g => g.PropertyTypes, expression => expression.Ignore())
                .AfterMap((source, destination) =>
                {
                     

                    destination.PropertyTypes = new PropertyTypeCollection();
                    foreach (var propertyTypeDisplay in source.Properties.Where(x => x.Label.IsNullOrWhiteSpace() == false ))
                    {
                        destination.PropertyTypes.Add(Mapper.Map<PropertyType>(propertyTypeDisplay));
                    }
                });


            config.CreateMap<PropertyTypeDisplay, PropertyType>()
                .ConstructUsing((PropertyTypeDisplay propertyTypeDisplay) =>
                {
                    var dataType = applicationContext.Services.DataTypeService.GetDataTypeDefinitionById(propertyTypeDisplay.DataTypeId);
                    if (dataType == null) throw new NullReferenceException("No data type found with id " + propertyTypeDisplay.DataTypeId);
                    return new PropertyType(dataType, propertyTypeDisplay.Alias);
                })
                .ForMember(dest => dest.Id, expression => expression.Condition(source => source.Id > 0))
                .ForMember(dto => dto.CreateDate, expression => expression.Ignore())
                .ForMember(dto => dto.UpdateDate, expression => expression.Ignore())
                .ForMember(type => type.PropertyGroupId, expression => expression.MapFrom(display => new Lazy<int>(() => display.GroupId, LazyThreadSafetyMode.None)))
                .ForMember(type => type.Key, expression => expression.Ignore())
                .ForMember(type => type.HelpText, expression => expression.Ignore())
                .ForMember(type => type.HasIdentity, expression => expression.Ignore())
                //ignore because this is set in the ctor
                .ForMember(type => type.Alias, expression => expression.Ignore())
                //ignore because this is obsolete and shouldn't be used
                .ForMember(type => type.DataTypeId, expression => expression.Ignore())
                .ForMember(type => type.Mandatory, expression => expression.MapFrom(display => display.Validation.Mandatory))
                .ForMember(type => type.ValidationRegExp, expression => expression.MapFrom(display => display.Validation.Pattern))
                .ForMember(type => type.PropertyEditorAlias, expression => expression.MapFrom(display => display.Editor))
                .ForMember(type => type.DataTypeDefinitionId, expression => expression.MapFrom(display => display.DataTypeId))
                .ForMember(type => type.Name, expression => expression.MapFrom(display => display.Label));
        }

        
    }
}