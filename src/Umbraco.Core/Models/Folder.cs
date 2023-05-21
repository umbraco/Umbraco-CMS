using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

public sealed class Folder : EntityBase
{
    public Folder(string folderPath) => Path = folderPath;

    public string Path { get; set; }
}
