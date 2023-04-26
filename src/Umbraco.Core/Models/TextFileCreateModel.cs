namespace Umbraco.Cms.Core.Models;

public class TextFileCreateModel
{
    public required string Name { get; set; }

    public string? ParentPath { get; set; }

    public string? Content { get; set; }

    public string FilePath =>
        ParentPath is null
            ? Name
            : Path.Combine(ParentPath, Name);
}
