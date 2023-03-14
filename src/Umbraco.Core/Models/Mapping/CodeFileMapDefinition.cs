using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Core.Models.Mapping;

public class CodeFileMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<IStylesheet, EntityBasic>((source, context) => new EntityBasic(), Map);
        mapper.Define<IStylesheet, CodeFileDisplay>((source, context) => new CodeFileDisplay(), Map);

        mapper.Define<IPartialView, EntityBasic>((source, context) => new EntityBasic(), Map);
        mapper.Define<IPartialView, CodeFileDisplay>((source, context) => new CodeFileDisplay(), Map);

        mapper.Define<IScript, EntityBasic>((source, context) => new EntityBasic(), Map);
        mapper.Define<IScript, CodeFileDisplay>((source, context) => new CodeFileDisplay(), Map);

        mapper.Define<CodeFileDisplay, IPartialView>(Map);
        mapper.Define<CodeFileDisplay, IScript>(Map);
    }

    // Umbraco.Code.MapAll -Trashed -Udi -Icon
    private static void Map(IStylesheet source, EntityBasic target, MapperContext context)
    {
        target.Alias = source.Alias;
        target.Id = source.Id;
        target.Key = source.Key;
        target.Name = source.Name;
        target.ParentId = -1;
        target.Path = source.Path;
    }

    // Umbraco.Code.MapAll -Trashed -Udi -Icon
    private static void Map(IScript source, EntityBasic target, MapperContext context)
    {
        target.Alias = source.Alias;
        target.Id = source.Id;
        target.Key = source.Key;
        target.Name = source.Name;
        target.ParentId = -1;
        target.Path = source.Path;
    }

    // Umbraco.Code.MapAll -Trashed -Udi -Icon
    private static void Map(IPartialView source, EntityBasic target, MapperContext context)
    {
        target.Alias = source.Alias;
        target.Id = source.Id;
        target.Key = source.Key;
        target.Name = source.Name;
        target.ParentId = -1;
        target.Path = source.Path;
    }

    // Umbraco.Code.MapAll -FileType -Notifications -Path -Snippet
    private static void Map(IPartialView source, CodeFileDisplay target, MapperContext context)
    {
        target.Content = source.Content;
        target.Id = source.Id.ToString();
        target.Name = source.Name;
        target.VirtualPath = source.VirtualPath;
    }

    // Umbraco.Code.MapAll -FileType -Notifications -Path -Snippet
    private static void Map(IScript source, CodeFileDisplay target, MapperContext context)
    {
        target.Content = source.Content;
        target.Id = source.Id.ToString();
        target.Name = source.Name;
        target.VirtualPath = source.VirtualPath;
    }

    // Umbraco.Code.MapAll -FileType -Notifications -Path -Snippet
    private static void Map(IStylesheet source, CodeFileDisplay target, MapperContext context)
    {
        target.Content = source.Content;
        target.Id = source.Id.ToString();
        target.Name = source.Name;
        target.VirtualPath = source.VirtualPath;
    }

    // Umbraco.Code.MapAll -CreateDate -DeleteDate -UpdateDate
    // Umbraco.Code.MapAll -Id -Key -Alias -Name -OriginalPath -Path
    private static void Map(CodeFileDisplay source, IPartialView target, MapperContext context)
    {
        target.Content = source.Content;
        target.VirtualPath = source.VirtualPath;
    }

    // Umbraco.Code.MapAll -CreateDate -DeleteDate -UpdateDate -GetFileContent
    // Umbraco.Code.MapAll -Id -Key -Alias -Name -OriginalPath -Path
    private static void Map(CodeFileDisplay source, IScript target, MapperContext context)
    {
        target.Content = source.Content;
        target.VirtualPath = source.VirtualPath;
    }
}
