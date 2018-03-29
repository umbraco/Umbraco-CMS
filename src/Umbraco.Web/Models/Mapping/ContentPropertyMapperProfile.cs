using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// A mapper which declares how to map content properties. These mappings are shared among media (and probably members) which is
    /// why they are in their own mapper
    /// </summary>
    internal class ContentPropertyMapperProfile : Profile
    {
        public ContentPropertyMapperProfile(IDataTypeService dataTypeService, ILocalizedTextService textService)
        {
            var contentPropertyBasicConverter = new ContentPropertyBasicConverter<ContentPropertyBasic>(dataTypeService);
            var contentPropertyDtoConverter = new ContentPropertyDtoConverter(dataTypeService);
            var contentPropertyDisplayConverter = new ContentPropertyDisplayConverter(dataTypeService, textService);

            //FROM Property TO ContentPropertyBasic
            CreateMap<PropertyGroup, Tab<ContentPropertyDisplay>>()
                .ForMember(tab => tab.Label, expression => expression.MapFrom(@group => @group.Name))
                .ForMember(tab => tab.IsActive, expression => expression.UseValue(true))
                .ForMember(tab => tab.Properties, expression => expression.Ignore())
                .ForMember(tab => tab.Alias, expression => expression.Ignore());

            //FROM Property TO ContentPropertyBasic
            CreateMap<Property, ContentPropertyBasic>().ConvertUsing(contentPropertyBasicConverter);

            //FROM Property TO ContentPropertyDto
            CreateMap<Property, ContentPropertyDto>().ConvertUsing(contentPropertyDtoConverter);

            //FROM Property TO ContentPropertyDisplay
            CreateMap<Property, ContentPropertyDisplay>().ConvertUsing(contentPropertyDisplayConverter);
        }
    }
}
