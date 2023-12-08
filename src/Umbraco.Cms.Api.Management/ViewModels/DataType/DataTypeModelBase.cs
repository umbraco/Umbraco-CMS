namespace Umbraco.Cms.Api.Management.ViewModels.DataType;

public abstract class DataTypeModelBase
{
    public string Name { get; set; } = string.Empty;

    public string EditorAlias { get; set; } = string.Empty;

    public string? EditorUiAlias { get; set; }

    public IEnumerable<DataTypePropertyPresentationModel> Values { get; set; } = null!;
}
