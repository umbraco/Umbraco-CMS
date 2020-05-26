namespace Umbraco.Core.Models
{
    /// <summary>
    /// These are options that are passed to the IImageUrlGenerator implementation to determine
    /// the propery URL that is needed
    /// </summary>
    public class ImageUrlGenerationOptions
    {
        public ImageUrlGenerationOptions (string imageUrl)
        {
            ImageUrl = imageUrl;
        }

        public string ImageUrl { get; }
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

        /// <summary>
        /// The focal point position, in whatever units the registered IImageUrlGenerator uses,
        /// typically a percentage of the total image from 0.0 to 1.0.
        /// </summary>
        public class FocalPointPosition
        {
            public FocalPointPosition (decimal top, decimal left)
            {
                Left = left;
                Top = top;
            }

            public decimal Left { get; }
            public decimal Top { get; }
        }

        /// <summary>
        /// The bounds of the crop within the original image, in whatever units the registered
        /// IImageUrlGenerator uses, typically a percentage between 0 and 100.
        /// </summary>
        public class CropCoordinates
        {
            public CropCoordinates (decimal x1, decimal y1, decimal x2, decimal y2)
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
    }
}
