namespace Umbraco.Cms.Api.Management.ViewModels.DynamicRoot;

public class DynamicRootContextRequestModel
{
    public Guid? Id { get; set; }

    public required ReferenceByIdModel Parent { get; set; }

    public string? Culture { get; set; }

    public string? Segment { get; set; }
}
