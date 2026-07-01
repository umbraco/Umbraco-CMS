using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Represents a factory responsible for creating presentation models for data types.
/// </summary>
public interface IDataTypePresentationFactory
{

    /// <summary>
    /// Asynchronously creates a new data type based on the provided request model.
    /// </summary>
    /// <param name="requestModel">The model containing the details required to create the data type.</param>
    /// <returns>A task representing the asynchronous operation, containing an <see cref="Attempt{IDataType, DataTypeOperationStatus}"/> that indicates the result of the creation attempt and its status.</returns>
    Task<Attempt<IDataType, DataTypeOperationStatus>> CreateAsync(CreateDataTypeRequestModel requestModel);

    /// <summary>
    /// Asynchronously updates an existing data type based on the provided request model.
    /// </summary>
    /// <param name="requestModel">The model containing the updated data type details.</param>
    /// <param name="current">The existing <see cref="IDataType"/> instance to update.</param>
    /// <returns>A task representing the asynchronous operation, containing an <see cref="Attempt{IDataType, DataTypeOperationStatus}"/> that indicates the result of the update attempt and its status.</returns>
    Task<Attempt<IDataType, DataTypeOperationStatus>> CreateAsync(UpdateDataTypeRequestModel requestModel, IDataType current);
}
