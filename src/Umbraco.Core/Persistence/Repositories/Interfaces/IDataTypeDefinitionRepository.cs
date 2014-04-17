using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IDataTypeDefinitionRepository : IRepositoryQueryable<int, IDataTypeDefinition>
    {
        PreValueCollection GetPreValuesCollectionByDataTypeId(int dataTypeId);
        string GetPreValueAsString(int preValueId);
    }
}