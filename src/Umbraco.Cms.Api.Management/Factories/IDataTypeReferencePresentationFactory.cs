using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IDataTypeReferencePresentationFactory
{
    IEnumerable<DataTypeReferenceResponseModel> CreateDataTypeReferenceViewModels(IReadOnlyDictionary<Udi, IEnumerable<string>> dataTypeUsages);
}
