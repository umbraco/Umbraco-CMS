using System;
using System.Collections.Generic;
using System.IO;
using SixLabors.ImageSharp;
using System.Text;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Infrastructure.Media
{
    public class ImageSharpImageService : IImageSharpImageService
    {

        public ImageFileModel GetImage(string imageOriginalPath, int width, int height)
        {
            byte[] imageContent;
            IImageFormat imageFormat;

            if (string.IsNullOrEmpty(imageOriginalPath)) return null;

            using (var image = Image.Load(imageOriginalPath, out imageFormat))
            {
                image.Mutate(x => x.Resize(width, height));

                using (var stream = new MemoryStream())
                {
                    image.Save(stream, imageFormat);
                    imageContent = stream.ToArray();
                }
            }

            return new ImageFileModel(imageContent, imageFormat.DefaultMimeType);
        }
    }
}
