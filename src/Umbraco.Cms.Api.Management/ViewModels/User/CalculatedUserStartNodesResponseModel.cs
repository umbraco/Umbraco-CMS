namespace Umbraco.Cms.Api.Management.ViewModels.User;

public class CalculatedUserStartNodesResponseModel
{
    public required Guid Id { get; init; }

    public ISet<ReferenceByIdModel> DocumentStartNodeIds { get; set; } = new HashSet<ReferenceByIdModel>();

    public bool HasDocumentRootAccess { get; set; }

    public ISet<ReferenceByIdModel> MediaStartNodeIds { get; set; } = new HashSet<ReferenceByIdModel>();

    public bool HasMediaRootAccess { get; set; }

    public ISet<ReferenceByIdModel> ElementStartNodeIds { get; set; } = new HashSet<ReferenceByIdModel>();

    public bool HasElementRootAccess { get; set; }
}
