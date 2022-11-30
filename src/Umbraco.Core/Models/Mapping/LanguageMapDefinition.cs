using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Core.Models.Mapping;

public class LanguageMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<ILanguage, EntityBasic>((source, context) => new EntityBasic(), Map);
        mapper.Define<ILanguage, ContentEditing.Language>((source, context) => new ContentEditing.Language(), Map);
        mapper.Define<IEnumerable<ILanguage>, IEnumerable<ContentEditing.Language>>((source, context) => new List<ContentEditing.Language>(), Map);
    }

    // Umbraco.Code.MapAll -Udi -Path -Trashed -AdditionalData -Icon
    private static void Map(ILanguage source, EntityBasic target, MapperContext context)
    {
        target.Name = source.CultureName;
        target.Key = source.Key;
        target.ParentId = -1;
        target.Alias = source.IsoCode;
        target.Id = source.Id;
    }

    // Umbraco.Code.MapAll
    private static void Map(ILanguage source, ContentEditing.Language target, MapperContext context)
    {
        target.Id = source.Id;
        target.IsoCode = source.IsoCode;
        target.Name = source.CultureName;
        target.IsDefault = source.IsDefault;
        target.IsMandatory = source.IsMandatory;
        target.FallbackLanguageId = source.FallbackLanguageId;
    }

    private static void Map(IEnumerable<ILanguage> source, IEnumerable<ContentEditing.Language> target, MapperContext context)
    {
        if (target == null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        if (!(target is List<ContentEditing.Language> list))
        {
            throw new NotSupportedException($"{nameof(target)} must be a List<Language>.");
        }

        List<ContentEditing.Language> temp = context.MapEnumerable<ILanguage, ContentEditing.Language>(source);

        // Put the default language first in the list & then sort rest by a-z
        ContentEditing.Language? defaultLang = temp.SingleOrDefault(x => x.IsDefault);

        // insert default lang first, then remaining language a-z
        if (defaultLang is not null)
        {
            list.Add(defaultLang);
            list.AddRange(temp.Where(x => x != defaultLang).OrderBy(x => x.Name));
        }
    }
}
