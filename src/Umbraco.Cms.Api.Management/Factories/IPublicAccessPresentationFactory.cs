using Umbraco.Cms.Api.Management.ViewModels.PublicAccess;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IPublicAccessPresentationFactory
{
    Attempt<PublicAccessResponseModel?, PublicAccessOperationStatus> CreatePublicAccessResponseModel(PublicAccessEntry entry);

    PublicAccessEntrySlim CreatePublicAccessEntrySlim(PublicAccessRequestModel requestModel, Guid contentKey);
}
