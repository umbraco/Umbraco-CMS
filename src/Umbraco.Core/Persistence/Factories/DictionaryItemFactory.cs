using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class DictionaryItemFactory : IEntityFactory<DictionaryItem, DictionaryDto>
    {
        #region Implementation of IEntityFactory<DictionaryItem,DictionaryDto>

        public DictionaryItem BuildEntity(DictionaryDto dto)
        {
            return new DictionaryItem(dto.Parent, dto.Key)
                       {
                           Id = dto.PrimaryKey, 
                           Key = dto.Id, 
                           Translations = BuildTranslations(dto)
                       };
        }

        public DictionaryDto BuildDto(DictionaryItem entity)
        {
            return new DictionaryDto
                       {
                           Id = entity.Key,
                           Key = entity.ItemKey,
                           Parent = entity.ParentId,
                           PrimaryKey = entity.Id,
                           LanguageTextDtos = BuildLanguageTextDtos(entity)
                       };
        }

        #endregion

        private List<LanguageTextDto> BuildLanguageTextDtos(DictionaryItem entity)
        {
            var list = new List<LanguageTextDto>();
            foreach (var translation in entity.Translations)
            {
                var text = new LanguageTextDto
                               {
                                   LanguageId = translation.Language.Id,
                                   UniqueId = translation.Key,
                                   Value = translation.Value
                               };

                if (translation.HasIdentity)
                    text.PrimaryKey = translation.Id;

                list.Add(text);
            }
            return list;
        }

        //NOTE Should probably be a callback in the repo, so the Language obj comes from the LanguageRepository
        private IEnumerable<DictionaryTranslation> BuildTranslations(DictionaryDto dto)
        {
            var list = new List<DictionaryTranslation>();
            foreach (var textDto in dto.LanguageTextDtos)
            {
                list.Add(new DictionaryTranslation(null, textDto.Value, dto.Id) {Id = textDto.PrimaryKey});
            }
            return list;
        }
    }
}