namespace Umbraco.Cms.Api.Management.ViewModels.DataType;

public class DataTypeViewModel
{
    public Guid Key { get; set; }

    public Guid? ParentKey { get; set; }

    public string Name { get; set; } = string.Empty;

    public string PropertyEditorAlias { get; set; } = string.Empty;

    public IEnumerable<DataTypePropertyViewModel> Configuration { get; set; } = null!;
}
