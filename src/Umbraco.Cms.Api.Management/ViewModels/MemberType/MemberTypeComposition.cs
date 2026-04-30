using Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.ViewModels.MemberType;

/// <summary>
/// Represents a view model for the composition of member types.
/// </summary>
public class MemberTypeComposition
{
    /// <summary>
    /// Gets or sets the reference to the composed member type.
    /// </summary>
    public required ReferenceByIdModel MemberType { get; init; }

    /// <summary>
    /// Gets or sets the type of composition applied to the member type, indicating how this member type is composed with others.
    /// </summary>
    public required CompositionType CompositionType { get; init; }
}
