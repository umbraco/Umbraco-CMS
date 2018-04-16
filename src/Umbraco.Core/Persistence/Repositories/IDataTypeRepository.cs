using System.Collections.Generic;
using Umbraco.Core.Events;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IDataTypeRepository : IReadWriteQueryRepository<int, IDataType>
    {
        IEnumerable<MoveEventInfo<IDataType>> Move(IDataType toMove, EntityContainer container);
    }
}
