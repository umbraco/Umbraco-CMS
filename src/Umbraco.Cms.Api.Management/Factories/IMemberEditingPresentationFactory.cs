using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Defines an interface for a factory that creates presentation models used for editing members.
/// </summary>
public interface IMemberEditingPresentationFactory
{
    /// <summary>
    /// Maps a <see cref="CreateMemberRequestModel"/> to a <see cref="MemberCreateModel"/>.
    /// </summary>
    /// <param name="createRequestModel">The create member request model to map from.</param>
    /// <returns>A <see cref="MemberCreateModel"/> representing the mapped member creation data.</returns>
    MemberCreateModel MapCreateModel(CreateMemberRequestModel createRequestModel);

    /// <summary>
    /// Maps the given <see cref="UpdateMemberRequestModel"/> to a <see cref="MemberUpdateModel"/>.
    /// </summary>
    /// <param name="updateRequestModel">The update request model containing member data to map.</param>
    /// <returns>The mapped <see cref="MemberUpdateModel"/> instance.</returns>
    MemberUpdateModel MapUpdateModel(UpdateMemberRequestModel updateRequestModel);
}
