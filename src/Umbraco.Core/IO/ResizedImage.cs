namespace Umbraco.Core.IO
{
    internal class ResizedImage
    {
        public ResizedImage()
        {
        }

        public ResizedImage(int width, int height, string fileName)
        {
            Width = width;
            Height = height;
            FileName = fileName;
        }

        public int Width { get; set; }
        public int Height { get; set; }
        public string FileName { get; set; }
    }
}