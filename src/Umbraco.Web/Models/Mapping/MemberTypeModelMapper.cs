using System;
using System.Linq;

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
    internal class MemberTypeModelMapper : MapperConfiguration
    {
        private readonly Lazy<PropertyEditorResolver> _propertyEditorResolver;
        
        //default ctor
        public MemberTypeModelMapper()
        {
            _propertyEditorResolver = new Lazy<PropertyEditorResolver>(() => PropertyEditorResolver.Current);
        }

        //ctor can be used for testing
        public MemberTypeModelMapper(Lazy<PropertyEditorResolver> propertyEditorResolver)
        {
            _propertyEditorResolver = propertyEditorResolver;
        }

        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            config.CreateMap<IMemberType, ContentTypeBasic>();
            config.CreateMap<IMemberType, MemberTypeDisplay>()
                //Ignore because this is not actually used for content types
                .ForMember(display => display.Trashed, expression => expression.Ignore())
                .ForMember(
                    dto => dto.Groups,
                    expression => expression.ResolveUsing(new PropertyTypeGroupResolver(applicationContext, _propertyEditorResolver)));

            config.CreateMap<MemberTypeDisplay, IMemberType>()
                .ConstructUsing((MemberTypeDisplay source) => new MemberType(source.ParentId))

                //only map id if set to something higher then zero
                .ForMember(dto => dto.Id, expression => expression.Condition(display => (Convert.ToInt32(display.Id) > 0)))
                .ForMember(dto => dto.Id, expression => expression.MapFrom(display => Convert.ToInt32(display.Id)))
                .ForMember(dto => dto.CreatorId, expression => expression.Ignore())
                .ForMember(dto => dto.Level, expression => expression.Ignore())
                .ForMember(dto => dto.SortOrder, expression => expression.Ignore())
               
                //mapped in aftermap
                .ForMember(dto => dto.AllowedContentTypes, expression => expression.Ignore())
            
                //ignore, we'll do this in after map
                .ForMember(dto => dto.PropertyGroups, expression => expression.Ignore())
                .AfterMap((source, dest) =>
                {
                    //get all properties from groups that are not generic properties (-666 id)
                    foreach (var groupDisplay in source.Groups.Where(x => x.Id != -666))
                    {
                        //use underlying logic to add the property group which should wire most things up for us
                        dest.AddPropertyGroup(groupDisplay.Name);
                        //now update that group with the values from the display object
                        Mapper.Map(groupDisplay, dest.PropertyGroups[groupDisplay.Name]);

                        foreach (var propertyTypeDisplay in groupDisplay.Properties)
                        {
                            dest.AddPropertyType(Mapper.Map<PropertyType>(propertyTypeDisplay), groupDisplay.Name);
                        }
                        //dest.PropertyGroups.Add(Mapper.Map<PropertyGroup>(groupDisplay));
                    }

                    //add generic properties
                    var genericProperties = source.Groups.FirstOrDefault(x => x.Id == -666);
                    if (genericProperties != null)
                    {
                        foreach (var propertyTypeDisplay in genericProperties.Properties)
                        {
                            dest.AddPropertyType(Mapper.Map<PropertyType>(propertyTypeDisplay));
                        }
                    }

                  });
        }
    }
}
