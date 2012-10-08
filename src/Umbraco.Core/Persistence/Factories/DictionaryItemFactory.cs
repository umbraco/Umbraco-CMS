using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class DictionaryItemFactory : IEntityFactory<DictionaryItem, DictionaryDto>
    {
        #region Implementation of IEntityFactory<DictionaryItem,DictionaryDto>

        public DictionaryItem BuildEntity(DictionaryDto dto)
        {
            throw new System.NotImplementedException();
        }

        public DictionaryDto BuildDto(DictionaryItem entity)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}