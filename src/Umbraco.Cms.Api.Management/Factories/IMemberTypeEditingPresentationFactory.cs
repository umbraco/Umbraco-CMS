using Umbraco.Cms.Api.Management.ViewModels.MemberType;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Represents a factory responsible for creating presentation models for editing member types.
/// </summary>
public interface IMemberTypeEditingPresentationFactory
{
    /// <summary>
    /// Maps a <see cref="CreateMemberTypeRequestModel"/> to a <see cref="MemberTypeCreateModel"/>.
    /// </summary>
    /// <param name="requestModel">The request model containing data to create a member type.</param>
    /// <returns>The mapped <see cref="MemberTypeCreateModel"/> instance.</returns>
    MemberTypeCreateModel MapCreateModel(CreateMemberTypeRequestModel requestModel);

    /// <summary>
    /// Maps the given <see cref="UpdateMemberTypeRequestModel"/> to a <see cref="MemberTypeUpdateModel"/>.
    /// </summary>
    /// <param name="requestModel">The request model containing member type update data.</param>
    /// <returns>A <see cref="MemberTypeUpdateModel"/> representing the updated member type.</returns>
    MemberTypeUpdateModel MapUpdateModel(UpdateMemberTypeRequestModel requestModel);

    /// <summary>
    /// Maps a collection of content type available compositions to a collection of available member type composition response models.
    /// </summary>
    /// <param name="compositionResults">The collection of content type available compositions to map.</param>
    /// <returns>A collection of available member type composition response models.</returns>
    IEnumerable<AvailableMemberTypeCompositionResponseModel> MapCompositionModels(IEnumerable<ContentTypeAvailableCompositionsResult> compositionResults);
}
