namespace Umbraco.Core.Models
{
    public class ImageUrlGenerationOptions
    {
        public string ImageUrl { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public decimal? WidthRatio { get; set; }
        public decimal? HeightRatio { get; set; }
        public int? Quality { get; set; }
        public string ImageCropMode { get; set; }
        public string ImageCropAnchor { get; set; }
        public bool DefaultCrop { get; set; }
        public FocalPointPosition FocalPoint { get; set; }
        public CropCoordinates Crop { get; set; }
        public string CacheBusterValue { get; set; }
        public string FurtherOptions { get; set; }
        public bool UpScale { get; set; } = true;
        public string AnimationProcessMode { get; set; }

        public class FocalPointPosition
        {
            public decimal Left { get; set; }
            public decimal Top { get; set; }
        }

        public class CropCoordinates
        {
            public decimal X1 { get; set; }
            public decimal Y1 { get; set; }
            public decimal X2 { get; set; }
            public decimal Y2 { get; set; }
        }
    }
}
