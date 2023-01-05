namespace Umbraco.Cms.Api.Management.ViewModels.DataType;

public abstract class DataTypeModelBase
{
    public string Name { get; set; } = string.Empty;

    public string PropertyEditorAlias { get; set; } = string.Empty;

    public IEnumerable<DataTypePropertyViewModel> Data { get; set; } = null!;
}
