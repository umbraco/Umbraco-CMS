namespace Umbraco.Cms.Web.Common.ImageProcessors
{
    /// <summary>
    /// Represents the mode used to calculate a crop.
    /// </summary>
    public enum CropMode
    {
        /// <summary>
        /// Crops the image using the standard rectangle model of x, y, width, height.
        /// </summary>
        Pixels,

        /// <summary>
        /// Crops the image using the percentages model of left, top, right, bottom.
        /// </summary>
        Percentage
    }
}
