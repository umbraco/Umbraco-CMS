namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum ContentCollectionOperationStatus
{
    Success,
    CollectionNotFound,
    ContentNotCollection,
    ContentNotFound,
    ContentTypeNotFound,
    DataTypeNotCollection,
    DataTypeNotContentProperty,
    DataTypeNotFound,
    DataTypeWithoutContentType,
    MissingPropertiesInCollectionConfiguration,
    OrderByNotPartOfCollectionConfiguration
}
