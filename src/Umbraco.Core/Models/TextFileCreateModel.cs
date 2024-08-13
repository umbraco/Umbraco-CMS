namespace Umbraco.Cms.Core.Models;

public abstract class TextFileCreateModel
{
    public required string Name { get; set; }

    public string? ParentPath { get; set; }

    public string? Content { get; set; }
}
