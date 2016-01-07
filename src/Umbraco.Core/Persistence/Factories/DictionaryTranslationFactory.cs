using System;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class DictionaryTranslationFactory 
    {
        private readonly Guid _uniqueId;

        public DictionaryTranslationFactory(Guid uniqueId)
        {
            _uniqueId = uniqueId;
        }

        #region Implementation of IEntityFactory<DictionaryTranslation,LanguageTextDto>

        public IDictionaryTranslation BuildEntity(LanguageTextDto dto)
        {
            var item = new DictionaryTranslation(dto.LanguageId, dto.Value, _uniqueId) 
                                            {Id = dto.PrimaryKey};

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            item.ResetDirtyProperties(false);
            return item;
        }

        public LanguageTextDto BuildDto(IDictionaryTranslation entity)
        {
            var text = new LanguageTextDto
                           {
                               LanguageId = entity.LanguageId,
                               UniqueId = _uniqueId,
                               Value = entity.Value
                           };

            if (entity.HasIdentity)
                text.PrimaryKey = entity.Id;

            return text;
        }

        #endregion
    }
}