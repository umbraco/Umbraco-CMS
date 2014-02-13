namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IImagingCropSize
    {
        string Alias { get; }

        int Width { get; }

        int Height { get; }
    }
}