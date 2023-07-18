namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum ContentTypeOperationStatus
{
    Success,
    DuplicateAlias,
    InvalidAlias,
    InvalidPropertyTypeAlias,
    DuplicatePropertyTypeAlias,
    InvalidDataType,
    DataTypeNotFound,
    InvalidInheritance,
    InvalidComposition,
    CompositionTypeNotFound,
    NotFound,
    ParentNotFound,
}
