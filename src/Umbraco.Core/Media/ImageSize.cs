namespace Umbraco.Web.Media
{
    public struct ImageSize
    {
        public int Width { get; }
        public int Height { get; }


        public ImageSize(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}
