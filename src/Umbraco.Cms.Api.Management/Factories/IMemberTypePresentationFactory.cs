using Umbraco.Cms.Api.Management.ViewModels.MemberType;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Represents a factory interface for creating presentation models of member types.
/// </summary>
public interface IMemberTypePresentationFactory
{
    /// <summary>
    /// Creates a response model asynchronously from the given member type.
    /// </summary>
    /// <param name="memberType">The member type to create the response model from.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the member type response model.</returns>
    Task<MemberTypeResponseModel> CreateResponseModelAsync(IMemberType memberType);
}
