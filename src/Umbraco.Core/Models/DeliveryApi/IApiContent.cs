namespace Umbraco.Cms.Core.Models.DeliveryApi;

public interface IApiContent : IApiElement
{
    string? Name { get; }

    IApiContentRoute Route { get; }
}
