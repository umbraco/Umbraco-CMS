using Umbraco.Cms.Api.Management.ViewModels.MemberType;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IMemberTypePresentationFactory
{
    Task<MemberTypeResponseModel> CreateResponseModelAsync(IMemberType memberType);
}
