using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IDataTypePresentationFactory
{

    Task<Attempt<IDataType, DataTypeOperationStatus>> CreateAsync(CreateDataTypeRequestModel requestModel);
    Task<Attempt<IDataType, DataTypeOperationStatus>> CreateAsync(UpdateDataTypeRequestModel requestModel, IDataType current);
}
