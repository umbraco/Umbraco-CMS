namespace Umbraco.Cms.Api.Management.ViewModels.Document;

// FIXME: rename to ContentUrlInfo and move to .Content namespace if we want to prepare media etc. for variations
public class DocumentUrlInfo
{
    public required string? Culture { get; init; }

    public required string Url { get; init; }
}
