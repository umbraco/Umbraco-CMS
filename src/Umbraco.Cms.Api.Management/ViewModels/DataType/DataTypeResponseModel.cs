namespace Umbraco.Cms.Api.Management.ViewModels.DataType;

public class DataTypeResponseModel : DataTypeModelBase, INamedEntityPresentationModel
{
    public Guid Key { get; set; }

    public Guid? ParentKey { get; set; }
}
