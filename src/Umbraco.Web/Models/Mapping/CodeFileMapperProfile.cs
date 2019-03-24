using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using Stylesheet = Umbraco.Core.Models.Stylesheet;

namespace Umbraco.Web.Models.Mapping
{
    public class CodeFileMapperProfile : IMapperProfile
    {
        public void SetMaps(Mapper mapper)
        {
            mapper.SetMap<Stylesheet, EntityBasic>(source => new EntityBasic(), Map);
            mapper.SetMap<IPartialView, CodeFileDisplay>(source => new CodeFileDisplay(), Map);
            mapper.SetMap<Script, CodeFileDisplay>(source => new CodeFileDisplay(), Map);
            mapper.SetMap<Stylesheet, CodeFileDisplay>(source => new CodeFileDisplay(), Map);
            mapper.SetMap<CodeFileDisplay, IPartialView>(Map);
            mapper.SetMap<CodeFileDisplay, Script>(Map);

        }

        // Umbraco.Code.MapAll -Trashed -Udi -Icon
        private static void Map(Stylesheet source, EntityBasic target)
        {
            target.Alias = source.Alias;
            target.Id = source.Id;
            target.Key = source.Key;
            target.Name = source.Name;
            target.ParentId = -1;
            target.Path = source.Path;
        }

        // Umbraco.Code.MapAll -FileType -Notifications -Path -Snippet
        private static void Map(IPartialView source, CodeFileDisplay target)
        {
            target.Content = source.Content;
            target.Id = source.Id.ToString();
            target.Name = source.Name;
            target.VirtualPath = source.VirtualPath;
        }

        // Umbraco.Code.MapAll -FileType -Notifications -Path -Snippet
        private static void Map(Script source, CodeFileDisplay target)
        {
            target.Content = source.Content;
            target.Id = source.Id.ToString();
            target.Name = source.Name;
            target.VirtualPath = source.VirtualPath;
        }

        // Umbraco.Code.MapAll -FileType -Notifications -Path -Snippet
        private static void Map(Stylesheet source, CodeFileDisplay target)
        {
            target.Content = source.Content;
            target.Id = source.Id.ToString();
            target.Name = source.Name;
            target.VirtualPath = source.VirtualPath;
        }

        // Umbraco.Code.MapAll -CreateDate -DeleteDate -UpdateDate
        // Umbraco.Code.MapAll -Id -Key -Alias -Name -OriginalPath -Path
        private static void Map(CodeFileDisplay source, IPartialView target)
        {
            target.Content = source.Content;
            target.VirtualPath = source.VirtualPath;
        }

        // Umbraco.Code.MapAll -CreateDate -DeleteDate -UpdateDate -GetFileContent
        // Umbraco.Code.MapAll -Id -Key -Alias -Name -OriginalPath -Path
        private static void Map(CodeFileDisplay source, Script target)
        {
            target.Content = source.Content;
            target.VirtualPath = source.VirtualPath;
        }
    }
}
