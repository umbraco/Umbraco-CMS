namespace Umbraco.Cms.Core.Models.FileSystem;

public abstract class FolderCreateModel
{
    public required string Name { get; set; }

    public string? ParentPath { get; set; }
}
