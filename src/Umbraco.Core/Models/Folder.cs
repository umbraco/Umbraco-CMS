using Umbraco.Core.CodeAnnotations;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    [UmbracoVolatile]
    public sealed class Folder : EntityBase
    {
        public Folder(string folderPath)
        {
            Path = folderPath;
        }

        public string Path { get; set; }
    }
}
