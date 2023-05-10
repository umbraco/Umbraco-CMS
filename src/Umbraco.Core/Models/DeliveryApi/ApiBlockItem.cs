namespace Umbraco.Cms.Core.Models.DeliveryApi;

public class ApiBlockItem
{
    public ApiBlockItem(IApiElement content, IApiElement? settings)
    {
        Content = content;
        Settings = settings;
    }

    public IApiElement Content { get; }

    public IApiElement? Settings { get; }
}
