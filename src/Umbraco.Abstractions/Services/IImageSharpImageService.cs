using System;
using System.Collections.Generic;
using System.Text;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public interface IImageSharpImageService
    {
        ImageFileModel GetImage(string imageOriginalPath, int width, int height);
    }
}
