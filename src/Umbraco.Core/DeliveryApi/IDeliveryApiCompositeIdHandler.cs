namespace Umbraco.Cms.Core.DeliveryApi;

public interface IDeliveryApiCompositeIdHandler
{
    string IndexId(int id, string culture);

    DeliveryApiIndexCompositeIdModel Decompose(string indexId);
}
