using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IDataTypeDefinitionRepository : IRepositoryQueryable<int, IDataTypeDefinition>
    {
        EntityContainer CreateContainer(int parentId, string name, int userId);
        void DeleteContainer(int containerId);

        PreValueCollection GetPreValuesCollectionByDataTypeId(int dataTypeId);
        string GetPreValueAsString(int preValueId);

        void AddOrUpdatePreValues(IDataTypeDefinition dataType, IDictionary<string, PreValue> values);
        void AddOrUpdatePreValues(int dataTypeId, IDictionary<string, PreValue> values);
    }
}