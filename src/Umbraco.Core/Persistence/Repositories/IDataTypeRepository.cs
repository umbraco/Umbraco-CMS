using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IDataTypeRepository : IReadWriteQueryRepository<int, IDataType>
{
    IEnumerable<MoveEventInfo<IDataType>> Move(IDataType toMove, EntityContainer? container);

    /// <summary>
    ///     Returns a dictionary of content type <see cref="Udi" />s and the property type aliases that use a
    ///     <see cref="IDataType" />
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    IReadOnlyDictionary<Udi, IEnumerable<string>> FindUsages(int id);
}
