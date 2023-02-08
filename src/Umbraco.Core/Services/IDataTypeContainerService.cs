﻿using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IDataTypeContainerService
{
    /// <summary>
    /// Gets a data type container
    /// </summary>
    /// <param name="id">The ID of the data type container to get.</param>
    /// <returns></returns>
    Task<EntityContainer?> GetAsync(Guid id);

    /// <summary>
    /// Gets the parent container of a data type container
    /// </summary>
    /// <param name="container">The container whose parent container to get.</param>
    /// <returns></returns>
    Task<EntityContainer?> GetParentAsync(EntityContainer container);

    /// <summary>
    /// Creates a new data type container
    /// </summary>
    /// <param name="container">The container to create.</param>
    /// <param name="parentId">The ID of the parent container to create the new container under.</param>
    /// <param name="userId">The ID of the user issuing the creation.</param>
    /// <returns></returns>
    /// <remarks>If no parent ID is supplied, the container will be created at the data type tree root.</remarks>
    Task<Attempt<EntityContainer, DataTypeContainerOperationStatus>> CreateAsync(EntityContainer container, Guid? parentId = null, int userId = Constants.Security.SuperUserId);

    /// <summary>
    /// Updates an existing data type container
    /// </summary>
    /// <param name="container">The container to create.</param>
    /// <param name="userId">The ID of the user issuing the update.</param>
    /// <returns></returns>
    Task<Attempt<EntityContainer, DataTypeContainerOperationStatus>> UpdateAsync(EntityContainer container, int userId = Constants.Security.SuperUserId);

    /// <summary>
    /// Deletes a data type container
    /// </summary>
    /// <param name="id">The ID of the container to delete.</param>
    /// <param name="userId">The ID of the user issuing the deletion.</param>
    /// <returns></returns>
    Task<Attempt<EntityContainer?, DataTypeContainerOperationStatus>> DeleteAsync(Guid id, int userId = Constants.Security.SuperUserId);
}
