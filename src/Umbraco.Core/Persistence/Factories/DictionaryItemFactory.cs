using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Factories
{
    internal static class DictionaryItemFactory
    {
        #region Implementation of IEntityFactory<DictionaryItem,DictionaryDto>

        public static IDictionaryItem BuildEntity(DictionaryDto dto)
        {
            var item = new DictionaryItem(dto.Parent, dto.Key);

            try
            {
                item.DisableChangeTracking();

                item.Id = dto.PrimaryKey;
                item.Key = dto.UniqueId;

                // reset dirty initial properties (U4-1946)
                item.ResetDirtyProperties(false);
                return item;
            }
            finally
            {
                item.EnableChangeTracking();
            }
        }

        public static DictionaryDto BuildDto(IDictionaryItem entity)
        {
            return new DictionaryDto
                       {
                           UniqueId = entity.Key,
                           Key = entity.ItemKey,
                           Parent = entity.ParentId,
                           PrimaryKey = entity.Id,
                           LanguageTextDtos = BuildLanguageTextDtos(entity)
                       };
        }

        #endregion

        private static List<LanguageTextDto> BuildLanguageTextDtos(IDictionaryItem entity)
        {
            var list = new List<LanguageTextDto>();
            foreach (var translation in entity.Translations)
            {
                var text = new LanguageTextDto
                               {
                                   LanguageId = translation.LanguageId,
                                   UniqueId = translation.Key,
                                   Value = translation.Value
                               };

                if (translation.HasIdentity)
                    text.PrimaryKey = translation.Id;

                list.Add(text);
            }
            return list;
        }
    }
}
