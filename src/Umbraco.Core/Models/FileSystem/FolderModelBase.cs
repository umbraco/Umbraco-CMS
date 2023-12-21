namespace Umbraco.Cms.Core.Models.FileSystem;

public abstract class FolderModelBase
{
    public string Name { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public string? ParentPath { get; set; }
}
