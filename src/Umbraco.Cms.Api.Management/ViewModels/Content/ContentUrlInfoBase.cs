namespace Umbraco.Cms.Api.Management.ViewModels.Content;

public abstract class ContentUrlInfoBase
{
    public required string? Culture { get; init; }

    public required string Url { get; init; }
}
