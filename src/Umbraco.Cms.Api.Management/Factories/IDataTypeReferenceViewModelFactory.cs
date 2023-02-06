using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IDataTypeReferenceViewModelFactory
{
    IEnumerable<DataTypeReferenceViewModel> CreateDataTypeReferenceViewModels(IReadOnlyDictionary<Udi, IEnumerable<string>> dataTypeUsages);
}
