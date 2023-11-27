using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Examine;

public class DeliveryApiCompositeIdHandler : IDeliveryApiCompositeIdHandler
{
    public string IndexId(int id, string culture) => $"{id}|{culture}";

    public DeliveryApiIndexCompositeIdModel Decompose(string indexId)
    {
        var parts = indexId.Split(Constants.CharArrays.VerticalTab);
        if (parts.Length == 2 && int.TryParse(parts[0], out _))
        {
            return new DeliveryApiIndexCompositeIdModel
            {
                Id = parts[0],
                Culture = parts[1],
            };
        }

        return new DeliveryApiIndexCompositeIdModel();
    }
}
