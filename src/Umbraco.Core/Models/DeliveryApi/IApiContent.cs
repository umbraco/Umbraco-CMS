namespace Umbraco.Cms.Core.Models.DeliveryApi;

public interface IApiContent : IApiElement
{
    string? Name { get; }

    public DateTime CreateDate { get; }

    public DateTime UpdateDate { get; }

    IApiContentRoute Route { get; }
}
