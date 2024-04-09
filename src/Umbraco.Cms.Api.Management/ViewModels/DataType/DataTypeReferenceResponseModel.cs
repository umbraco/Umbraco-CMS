namespace Umbraco.Cms.Api.Management.ViewModels.DataType;

public class DataTypeReferenceResponseModel
{
    public required Guid Id { get; init; }

    public required string Type { get; init; }

    public required IEnumerable<DataTypePropertyReferenceViewModel> Properties { get; init; }
}
