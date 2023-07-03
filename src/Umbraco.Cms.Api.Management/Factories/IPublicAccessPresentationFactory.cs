using Umbraco.Cms.Api.Management.ViewModels.PublicAccess;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IPublicAccessPresentationFactory
{
    Task<Attempt<PublicAccessResponseModel?, PublicAccessOperationStatus>> CreatePublicAccessResponseModel(PublicAccessEntry entry);
    Task<Attempt<PublicAccessEntry?, PublicAccessOperationStatus>> CreatePublicAccessEntry(PublicAccessRequestModel requestModel);

}
