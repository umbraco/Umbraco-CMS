namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum ContentTypeOperationStatus
{
    Success,
    DuplicateAlias,
    InvalidAlias,
    InvalidPropertyTypeAlias,
    InvalidDataType,
    DataTypeNotFound,
    InvalidInheritance,
    NotFound,
}
