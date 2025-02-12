namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum ContentTypeOperationStatus
{
    Success,
    DuplicateAlias,
    InvalidAlias,
    InvalidPropertyTypeAlias,
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
    PropertyTypeAliasCannotEqualContentTypeAlias,
    NameCannotBeEmpty,
    NameTooLong,
    InvalidElementFlagDocumentHasContent,
    InvalidElementFlagElementIsUsedInPropertyEditorConfiguration,
    InvalidElementFlagComparedToParent,
}
