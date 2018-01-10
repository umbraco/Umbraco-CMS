using System.Collections.Generic;
using Umbraco.Core.Events;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IDataTypeRepository : IReadWriteQueryRepository<int, IDataType>
    {

        PreValueCollection GetPreValuesCollectionByDataTypeId(int dataTypeId);
        string GetPreValueAsString(int preValueId);

        void AddOrUpdatePreValues(IDataType dataType, IDictionary<string, PreValue> values);
        void AddOrUpdatePreValues(int dataTypeId, IDictionary<string, PreValue> values);
        IEnumerable<MoveEventInfo<IDataType>> Move(IDataType toMove, EntityContainer container);
    }
}
