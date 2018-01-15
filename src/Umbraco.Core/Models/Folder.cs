using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    internal sealed class Folder : EntityBase
    {
        public Folder(string folderPath)
        {
            Path = folderPath;
        }

        public string Path { get; set; }
    }
}
