namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of a content collection operation.
/// </summary>
public enum ContentCollectionOperationStatus
{
    /// <summary>
    ///     The operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    ///     The specified collection was not found.
    /// </summary>
    CollectionNotFound,

    /// <summary>
    ///     The content item is not configured as a collection.
    /// </summary>
    ContentNotCollection,

    /// <summary>
    ///     The specified content item was not found.
    /// </summary>
    ContentNotFound,

    /// <summary>
    ///     The specified content type was not found.
    /// </summary>
    ContentTypeNotFound,

    /// <summary>
    ///     The data type is not configured for collection use.
    /// </summary>
    DataTypeNotCollection,

    /// <summary>
    ///     The data type is not a content property.
    /// </summary>
    DataTypeNotContentProperty,

    /// <summary>
    ///     The specified data type was not found.
    /// </summary>
    DataTypeNotFound,

    /// <summary>
    ///     The data type does not have an associated content type.
    /// </summary>
    DataTypeWithoutContentType,

    /// <summary>
    ///     Required properties are missing from the collection configuration.
    /// </summary>
    MissingPropertiesInCollectionConfiguration,

    /// <summary>
    ///     The specified order by field is not part of the collection configuration.
    /// </summary>
    OrderByNotPartOfCollectionConfiguration
}
