namespace Umbraco.Cms.Api.Management.ViewModels.DataType;

public class DataTypeReferenceViewModel
{
    public required Guid Key { get; init; }

    public required string Type { get; init; }

    public required IEnumerable<DataTypePropertyReferenceViewModel> Properties { get; init; }
}
