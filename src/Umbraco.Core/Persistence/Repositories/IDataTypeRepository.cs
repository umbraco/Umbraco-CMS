using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="IDataType" /> entities.
/// </summary>
public interface IDataTypeRepository : IReadWriteQueryRepository<int, IDataType>, IReadRepository<Guid, IDataType>
{
    /// <summary>
    ///     Moves a data type to a container.
    /// </summary>
    /// <param name="toMove">The data type to move.</param>
    /// <param name="container">The target container, or <c>null</c> to move to the root.</param>
    /// <returns>A collection of move event information.</returns>
    IEnumerable<MoveEventInfo<IDataType>> Move(IDataType toMove, EntityContainer? container);

    /// <summary>
    ///     Returns a dictionary of content type <see cref="Udi" />s and the property type aliases that use a
    ///     <see cref="IDataType" />
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    IReadOnlyDictionary<Udi, IEnumerable<string>> FindUsages(int id);

    /// <summary>
    ///     Returns a dictionary of content type <see cref="Udi" />s and the data type (List view) aliases that use a
    ///     <see cref="IDataType" />
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    IReadOnlyDictionary<Udi, IEnumerable<string>> FindListViewUsages(int id) => throw new NotImplementedException();
}
