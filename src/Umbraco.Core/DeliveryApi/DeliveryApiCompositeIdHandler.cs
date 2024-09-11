namespace Umbraco.Cms.Core.DeliveryApi;

public class DeliveryApiCompositeIdHandler : IDeliveryApiCompositeIdHandler
{
    public string IndexId(int id, string culture) => $"{id}|{culture}";

    public DeliveryApiIndexCompositeIdModel Decompose(string indexId)
    {
        var parts = indexId.Split(Constants.CharArrays.VerticalTab);
        if (parts.Length == 2 && int.TryParse(parts[0], out var id))
        {
            return new DeliveryApiIndexCompositeIdModel
            {
                Id = id,
                Culture = parts[1],
            };
        }

        return new DeliveryApiIndexCompositeIdModel();
    }
}
