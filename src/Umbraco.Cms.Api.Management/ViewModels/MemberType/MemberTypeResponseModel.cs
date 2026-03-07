using Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.ViewModels.MemberType;

    /// <summary>
    /// Represents a response model containing data for a member type returned by the management API.
    /// </summary>
public class MemberTypeResponseModel : ContentTypeResponseModelBase<MemberTypePropertyTypeResponseModel, MemberTypePropertyTypeContainerResponseModel>
{
    /// <summary>
    /// Gets or sets the compositions associated with the member type.
    /// </summary>
    public IEnumerable<MemberTypeComposition> Compositions { get; set; } = Enumerable.Empty<MemberTypeComposition>();
}
