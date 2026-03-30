using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Represents a factory responsible for creating presentations of data type references.
/// </summary>
public interface IDataTypeReferencePresentationFactory
{
    /// <summary>
    /// Creates view models representing data type references based on the provided usages.
    /// </summary>
    /// <param name="dataTypeUsages">A dictionary mapping Udi keys to collections of usage strings for each data type.</param>
    /// <returns>An enumerable of <see cref="DataTypeReferenceResponseModel"/> representing the data type references.</returns>
    IEnumerable<DataTypeReferenceResponseModel> CreateDataTypeReferenceViewModels(IReadOnlyDictionary<Udi, IEnumerable<string>> dataTypeUsages);
}
