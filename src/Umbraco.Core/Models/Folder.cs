using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    internal sealed class Folder : EntityBase.EntityBase
    {
        public Folder(string folderPath)
        {
            Path = folderPath;
        }

        public string Path { get; set; }
    }
}
