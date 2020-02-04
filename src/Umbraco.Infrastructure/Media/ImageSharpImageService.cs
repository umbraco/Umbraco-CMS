using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Infrastructure.Media
{
    public class ImageSharpImageService : IImageSharpImageService
    {

        public ImageFileModel GetImage(string imageOriginalPath, int? width, int? height)
        {
            byte[] imageData;
            IImageFormat imageFormat;

            if (string.IsNullOrEmpty(imageOriginalPath)) return null;

            using (var image = Image.Load(imageOriginalPath, out imageFormat))
            {
                image.Mutate(x =>
                {
                    if (width != null)
                        if (height != null)
                            x.Resize((int) width, (int) height);
                });

                using (var stream = new MemoryStream())
                {
                    image.Save(stream, imageFormat);
                    imageData = stream.ToArray();
                }
            }

            return new ImageFileModel(imageData, imageFormat.DefaultMimeType);
        }
    }
}
