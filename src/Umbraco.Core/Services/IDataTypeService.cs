using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Defines the DataType Service, which is an easy access to operations involving <see cref="IDataType" />
/// </summary>
public interface IDataTypeService : IService
{
    /// <summary>
    ///     Gets a paged result of items which are in relation with the current data type.
    /// </summary>
    /// <param name="key">The identifier of the data type to retrieve relations for.</param>
    /// <param name="skip">The amount of items to skip</param>
    /// <param name="take">The amount of items to take.</param>
    /// <returns>A paged result of <see cref="RelationItemModel" /> objects.</returns>
    /// <remarks>
    /// Note that the model and method signature here aligns with with how we handle retrieval of concrete Umbraco
    /// relations based on documents, media and members in <see cref="ITrackedReferencesService"/>.
    /// The intention is that we align data type relations with these so they can be handled polymorphically at the management API
    /// and backoffice UI level.
    /// </remarks>
    Task<PagedModel<RelationItemModel>> GetPagedRelationsAsync(Guid key, int skip, int take);

    /// <summary>
    ///     Gets an <see cref="IDataType" /> by its Name
    /// </summary>
    /// <param name="name">Name of the <see cref="IDataType" /></param>
    /// <returns>
    ///     <see cref="IDataType" />
    /// </returns>
    Task<IDataType?> GetAsync(string name);

    /// <summary>
    ///     Gets an <see cref="IDataType" /> by its unique guid Id
    /// </summary>
    /// <param name="id">Unique guid Id of the DataType</param>
    /// <returns>
    ///     <see cref="IDataType" />
    /// </returns>
    Task<IDataType?> GetAsync(Guid id);

    /// <summary>
    /// Gets multiple <see cref="IDataType"/> objects by their unique keys.
    /// </summary>
    /// <param name="keys">The keys to get datatypes by.</param>
    /// <returns>An attempt with the requested data types.</returns>
    Task<IEnumerable<IDataType>> GetAllAsync(params Guid[] keys);

    /// <summary>
    /// Gets multiple <see cref="IDataType"/> objects by their unique keys.
    /// </summary>
    /// <param name="name">Name to filter by.</param>
    /// <param name="editorUiAlias">Editor ui alias to filter by.</param>
    /// <param name="editorAlias">Editor alias to filter by.</param>
    /// <param name="skip">Number of items to skip.</param>
    /// <param name="take">Number of items to take.</param>
    /// <returns>An attempt with the requested data types.</returns>
    Task<PagedModel<IDataType>> FilterAsync(string? name = null, string? editorUiAlias = null, string? editorAlias = null, int skip = 0, int take = 100);

    /// <summary>
    ///     Creates a new <see cref="IDataType" />
    /// </summary>
    /// <param name="dataType"><see cref="IDataType" /> to create</param>
    /// <param name="userKey">Key of the user issuing the creation</param>
    Task<Attempt<IDataType, DataTypeOperationStatus>> CreateAsync(IDataType dataType, Guid userKey);

    /// <summary>
    ///     Updates an existing <see cref="IDataType" />
    /// </summary>
    /// <param name="dataType"><see cref="IDataType" /> to update</param>
    /// <param name="userKey">Key of the user issuing the update</param>
    Task<Attempt<IDataType, DataTypeOperationStatus>> UpdateAsync(IDataType dataType, Guid userKey);

    /// <summary>
    ///     Deletes an <see cref="IDataType" />
    /// </summary>
    /// <remarks>
    ///     Please note that deleting a <see cref="IDataType" /> will remove
    ///     all the <see cref="IPropertyType" /> data that references this <see cref="IDataType" />.
    /// </remarks>
    /// <param name="id">The guid Id of the <see cref="IDataType" /> to delete</param>
    /// <param name="userKey">Key of the user issuing the deletion</param>
    Task<Attempt<IDataType?, DataTypeOperationStatus>> DeleteAsync(Guid id, Guid userKey);

    /// <summary>
    ///     Gets all <see cref="IDataType" /> for a given property editor.
    /// </summary>
    /// <param name="propertyEditorAlias">Alias of the property editor.</param>
    /// <returns>Collection of <see cref="IDataType" /> configured for the property editor.</returns>
    Task<IEnumerable<IDataType>> GetByEditorAliasAsync(string propertyEditorAlias);

    /// <summary>
    ///     Gets all <see cref="IDataType" /> for a given editor UI alias.
    /// </summary>
    /// <param name="editorUiAlias">The UI Alias to query by.</param>
    /// <returns>Collection of <see cref="IDataType" /> which has the UI alias.</returns>
    Task<IEnumerable<IDataType>> GetByEditorUiAlias(string editorUiAlias);

    /// <summary>
    /// Moves a <see cref="IDataType"/> to a given container
    /// </summary>
    /// <param name="toMove">The data type that will be moved</param>
    /// <param name="containerKey">The container key where the data type will be moved to.</param>
    /// <param name="userKey">The user that did the Move action</param>
    /// <returns>An attempt result with the moved data type and operation status.</returns>
    Task<Attempt<IDataType, DataTypeOperationStatus>> MoveAsync(IDataType toMove, Guid? containerKey, Guid userKey);

    /// <summary>
    /// Copies a <see cref="IDataType"/> to a given container
    /// </summary>
    /// <param name="toCopy">The data type that will be copied</param>
    /// <param name="containerKey">The container key where the data type will be copied to.</param>
    /// <param name="userKey">The user that did the Copy action</param>
    /// <returns>An attempt result with the copied data type and operation status.</returns>
    Task<Attempt<IDataType, DataTypeOperationStatus>> CopyAsync(IDataType toCopy, Guid? containerKey, Guid userKey);

    /// <summary>
    /// Performs validation for the configuration data of a given data type.
    /// </summary>
    /// <param name="dataType">The data type whose configuration to validate.</param>
    /// <returns>One or more <see cref="ValidationResult"/> if the configuration data is invalid, an empty collection otherwise.</returns>
    IEnumerable<ValidationResult> ValidateConfigurationData(IDataType dataType);

    /// <summary>
    ///     Gets all <see cref="IDataType" /> for a set of property editors.
    /// </summary>
    /// <param name="propertyEditorAlias">Aliases of the property editors.</param>
    /// <returns>Collection of <see cref="IDataType" /> configured for the property editors.</returns>
    Task<IEnumerable<IDataType>> GetByEditorAliasAsync(string[] propertyEditorAlias);
}
