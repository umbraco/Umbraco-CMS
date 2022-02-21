namespace Umbraco.Cms.Core.Models
{
    /// <summary>
    /// These are options that are passed to the IImageUrlGenerator implementation to determine the URL that is generated.
    /// </summary>
    public class ImageUrlGenerationOptions
    {
        public ImageUrlGenerationOptions(string? imageUrl) => ImageUrl = imageUrl;

        public string? ImageUrl { get; }

        public int? Width { get; set; }

        public int? Height { get; set; }

        public int? Quality { get; set; }

        public ImageCropMode? ImageCropMode { get; set; }

        public ImageCropAnchor? ImageCropAnchor { get; set; }

        public FocalPointPosition? FocalPoint { get; set; }

        public CropCoordinates? Crop { get; set; }

        public string? CacheBusterValue { get; set; }

        public string? FurtherOptions { get; set; }

        /// <summary>
        /// The focal point position, in whatever units the registered IImageUrlGenerator uses, typically a percentage of the total image from 0.0 to 1.0.
        /// </summary>
        public class FocalPointPosition
        {
            public FocalPointPosition(decimal left, decimal top)
            {
                Left = left;
                Top = top;
            }

            public decimal Left { get; }

            public decimal Top { get; }
        }

        /// <summary>
        /// The bounds of the crop within the original image, in whatever units the registered IImageUrlGenerator uses, typically a percentage between 0.0 and 1.0.
        /// </summary>
        public class CropCoordinates
        {
            public CropCoordinates(decimal left, decimal top, decimal right, decimal bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public decimal Left { get; }

            public decimal Top { get; }

            public decimal Right { get; }

            public decimal Bottom { get; }
        }
    }
}
