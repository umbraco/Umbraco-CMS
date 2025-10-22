using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class DocumentUrlInfo : ContentUrlInfoBase
{
    public required string? Message { get; init; }

    public required string Provider { get; init; }

    public required bool IsExternal { get; init; }
}
