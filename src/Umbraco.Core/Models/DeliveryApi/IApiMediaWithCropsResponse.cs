namespace Umbraco.Cms.Core.Models.DeliveryApi;

public interface IApiMediaWithCropsResponse : IApiMediaWithCrops
{
    public string Path { get; }

    public DateTime CreateDate { get; }

    public DateTime UpdateDate { get; }
}
