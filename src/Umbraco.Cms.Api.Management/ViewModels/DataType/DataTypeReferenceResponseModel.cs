namespace Umbraco.Cms.Api.Management.ViewModels.DataType;

public class DataTypeReferenceResponseModel
{
    public required DataTypeContentTypeReferenceModel ContentType { get; init; }

    public required IEnumerable<DataTypePropertyReferenceViewModel> Properties { get; init; }
}
