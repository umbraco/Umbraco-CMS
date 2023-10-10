namespace Umbraco.Cms.Api.Management.ViewModels.Item;

public abstract class ItemResponseModelBase : IResponseModel
{
    public string Name { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public abstract string Type { get; }
}
