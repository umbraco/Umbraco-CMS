namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IDataTypeUsageRepository
{
    bool HasSavedValues(int dataTypeId);
}
