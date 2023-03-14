namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IPropertyTypeUsageRepository
{
    bool HasSavedPropertyValues(string propertyTypeAlias);
}
