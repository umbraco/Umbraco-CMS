namespace Umbraco.Cms.Infrastructure.Examine;

public interface IDeliveryApiCompositeIdHandler
{
    string IndexId(int id, string culture);

    DeliveryApiIndexCompositeIdModel Decompose(string indexId);
}
