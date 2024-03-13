namespace Umbraco.Cms.Core.Models.DeliveryApi;

public class ImageCropCoordinates
{
    public ImageCropCoordinates(decimal x1, decimal y1, decimal x2, decimal y2)
    {
        X1 = x1;
        Y1 = y1;
        X2 = x2;
        Y2 = y2;
    }

    public decimal X1 { get; }

    public decimal Y1 { get; }

    public decimal X2 { get; }

    public decimal Y2 { get; }
}
