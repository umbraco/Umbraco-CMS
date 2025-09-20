namespace Umbraco.Cms.Core.Models.DeliveryApi;

public class ImageCrop
{
    public ImageCrop(string? alias, int width, int height, ImageCropCoordinates? coordinates)
    {
        Alias = alias;
        Width = width;
        Height = height;
        Coordinates = coordinates;
    }

    public string? Alias { get; }

    public int Width { get; }

    public int Height { get; }

    public ImageCropCoordinates? Coordinates { get; }
}
