using Umbraco.Cms.Api.Management.ViewModels.MemberType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class MemberTypePresentationFactory : IMemberTypePresentationFactory
{
    private readonly IUmbracoMapper _umbracoMapper;

    public MemberTypePresentationFactory(IUmbracoMapper umbracoMapper)
        => _umbracoMapper = umbracoMapper;

    public Task<MemberTypeResponseModel> CreateResponseModelAsync(IMemberType memberType)
    {
        MemberTypeResponseModel model = _umbracoMapper.Map<MemberTypeResponseModel>(memberType)!;

        foreach (MemberTypePropertyTypeResponseModel propertyType in model.Properties)
        {
            propertyType.IsSensitive = memberType.IsSensitiveProperty(propertyType.Alias);

            propertyType.Visibility.MemberCanEdit = memberType.MemberCanEditProperty(propertyType.Alias);
            propertyType.Visibility.MemberCanView = memberType.MemberCanViewProperty(propertyType.Alias);
        }

        return Task.FromResult(model);
    }
}
