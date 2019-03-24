using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using Language = Umbraco.Web.Models.ContentEditing.Language;

namespace Umbraco.Web.Models.Mapping
{
    internal class LanguageMapperProfile : IMapperProfile
    {
        public void SetMaps(Mapper mapper)
        {
            mapper.SetMap<ILanguage, EntityBasic>(source => new EntityBasic(), Map);
            mapper.SetMap<ILanguage, Language>(source => new Language(), Map);
            mapper.SetMap<IEnumerable<ILanguage>, IEnumerable<Language>>(source => new List<Language>(), (source, target) => Map(source, target, mapper));
        }

        // Umbraco.Code.MapAll -Udi -Path -Trashed -AdditionalData -Icon
        private static void Map(ILanguage source, EntityBasic target)
        {
            target.Name = source.CultureName;
            target.Key = source.Key;
            target.ParentId = -1;
            target.Alias = source.IsoCode;
            target.Id = source.Id;
        }

        // Umbraco.Code.MapAll
        private static void Map(ILanguage source, Language target)
        {
            target.Id = source.Id;
            target.IsoCode = source.IsoCode;
            target.Name = source.CultureInfo.DisplayName;
            target.IsDefault = source.IsDefault;
            target.IsMandatory = source.IsMandatory;
            target.FallbackLanguageId = source.FallbackLanguageId;
        }

        private static void Map(IEnumerable<ILanguage> source, IEnumerable<Language> target, Mapper mapper)
        {
            if (!(target is List<Language> list))
                throw new NotSupportedException($"{nameof(target)} must be a List<Language>.");

            var temp = source.Select(mapper.Map<ILanguage, Language>).ToList();

            //Put the default language first in the list & then sort rest by a-z
            var defaultLang = temp.SingleOrDefault(x => x.IsDefault);

            // insert default lang first, then remaining language a-z
            list.Add(defaultLang);
            list.AddRange(temp.Where(x => x != defaultLang).OrderBy(x => x.Name));
        }
    }
}
