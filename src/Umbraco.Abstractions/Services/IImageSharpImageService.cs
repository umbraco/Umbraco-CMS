using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public interface IImageSharpImageService
    {
        ImageFileModel GetImage(string imageOriginalPath, int? width, int? height);
    }
}
