namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum ContentTypeOperationStatus
{
    Success,
    DuplicateAlias,
    InvalidAlias,
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
    InvalidElementFlagDocumentHasContent,
    InvalidElementFlagElementIsUsedInPropertyEditorConfiguration,
    InvalidElementFlagComparedToParent
}
