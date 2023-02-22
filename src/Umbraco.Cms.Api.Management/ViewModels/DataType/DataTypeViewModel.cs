namespace Umbraco.Cms.Api.Management.ViewModels.DataType;

public class DataTypeViewModel : DataTypeModelBase, INamedEntityViewModel
{
    public Guid Key { get; set; }

    public Guid? ParentKey { get; set; }
}
