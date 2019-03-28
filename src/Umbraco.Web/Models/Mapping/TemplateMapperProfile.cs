using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class TemplateMapperProfile : IMapperProfile
    {
        public void DefineMaps(Mapper mapper)
        {
            mapper.Define<ITemplate, TemplateDisplay>((source, context) => new TemplateDisplay(), Map);
            mapper.Define<TemplateDisplay, Template>((source, context) => new Template(source.Name, source.Alias), Map);
        }

        // Umbraco.Code.MapAll
        private static void Map(ITemplate source, TemplateDisplay target, MapperContext context)
        {
            target.Id = source.Id;
            target.Name = source.Name;
            target.Alias = source.Alias;
            target.Key = source.Key;
            target.Content = source.Content;
            target.Path = source.Path;
            target.VirtualPath = source.VirtualPath;
            target.MasterTemplateAlias = source.MasterTemplateAlias;
            target.IsMasterTemplate = source.IsMasterTemplate;
        }

        // Umbraco.Code.MapAll -CreateDate -UpdateDate -DeleteDate
        // Umbraco.Code.MapAll -Path -VirtualPath -MasterTemplateId -IsMasterTemplate
        // Umbraco.Code.MapAll -GetFileContent
        private static void Map(TemplateDisplay source, Template target, MapperContext context)
        {
            target.MasterTemplateAlias = source.MasterTemplateAlias;
            target.Name = source.Name;
            target.Alias = source.Alias;
            target.Content = source.Content;
            target.Id = source.Id;
            target.Key = source.Key;
        }
    }
}
