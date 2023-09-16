namespace Umbraco.Cms.Core.Models.DeliveryApi;

public class ImageFocalPoint
{
    public ImageFocalPoint(decimal left, decimal top)
    {
        Left = left;
        Top = top;
    }

    public decimal Left { get; }

    public decimal Top { get; }
}
