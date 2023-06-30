using Umbraco.Cms.Api.Management.ViewModels.PublicAccess;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IPublicAccessPresentationFactory
{
    PublicAccessResponseModel CreatePublicAccessResponseModel(PublicAccessEntry entry);
}
