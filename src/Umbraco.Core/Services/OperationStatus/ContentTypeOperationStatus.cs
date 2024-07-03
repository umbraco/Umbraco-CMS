namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum ContentTypeOperationStatus
{
    Success,
    DuplicateAlias,
    InvalidAlias,
    NameCannotBeEmpty,
    NameTooLong,
    InvalidPropertyTypeAlias,
    PropertyTypeAliasCannotEqualContentTypeAlias,
    DuplicatePropertyTypeAlias,
    DataTypeNotFound,
    InvalidInheritance,
    InvalidComposition,
    InvalidParent,
    InvalidContainerName,
    InvalidContainerType,
    MissingContainer,
    DuplicateContainer,
    NotFound,
    NotAllowed,
    CancelledByNotification,
}
