using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class DocumentUrlInfo : ContentUrlInfoBase
{
    public required string? Message { get; init; }
}
