namespace Umbraco.Cms.Api.Management.ViewModels.DataType;

public class DataTypeContentTypeReferenceModel
{
    public required Guid Id { get; set; }

    public required string? Type { get; set; }

    public required string? Name { get; set; }

    public required string? Icon { get; set; }
}
