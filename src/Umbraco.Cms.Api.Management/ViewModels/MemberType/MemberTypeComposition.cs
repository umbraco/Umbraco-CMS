using Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.ViewModels.MemberType;

public class MemberTypeComposition
{
    public required ReferenceByIdModel MemberType { get; init; }

    public required CompositionType CompositionType { get; init; }
}
