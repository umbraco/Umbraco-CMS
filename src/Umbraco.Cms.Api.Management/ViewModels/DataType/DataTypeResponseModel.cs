namespace Umbraco.Cms.Api.Management.ViewModels.DataType;

public class DataTypeResponseModel : DataTypeModelBase
{
    public Guid Id { get; set; }

    public bool IsDeletable { get; set; }

    public bool CanIgnoreStartNodes { get; set; }
}
