namespace Umbraco.Cms.Api.Management.ViewModels.DataType;

public class DataTypeViewModel : DataTypeModelBase
{
    public Guid Key { get; set; }

    public Guid? ParentKey { get; set; }
}
