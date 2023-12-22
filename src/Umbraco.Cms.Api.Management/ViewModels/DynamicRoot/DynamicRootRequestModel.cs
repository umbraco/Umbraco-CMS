namespace Umbraco.Cms.Api.Management.ViewModels.DynamicRoot;

public class DynamicRootRequestModel
{
    public DynamicRootContextRequestModel Context { get; set; } = null!;
    public DynamicRootQueryRequestModel Query { get; set; } = null!;
}


public class DynamicRootContextRequestModel
{
    public Guid? Id { get; set; }
    public Guid ParentId { get; set; } = Guid.Empty;
    public string Culture { get; set; } = string.Empty;
    public string Segment { get; set; } = string.Empty;
}


public class DynamicRootQueryRequestModel
{
    public DynamicRootQueryOriginRequestModel Origin { get; set; } = null!;

    public DynamicRootQueryStepRequestModel[] QuerySteps { get; set; } = Array.Empty<DynamicRootQueryStepRequestModel>();
}

public class DynamicRootQueryOriginRequestModel
{
    public string Alias { get; set; } = string.Empty;

    public Guid? Key { get; set; }

}

public class DynamicRootQueryStepRequestModel
{
    public string Alias { get; set; } = string.Empty;

    public IEnumerable<Guid> AnyOfDocTypeKeys { get; set; } = Array.Empty<Guid>();
}

