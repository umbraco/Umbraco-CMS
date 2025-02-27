using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Api.Management.ViewModels.MemberType;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IConfigurationPresentationFactory
{
    DocumentConfigurationResponseModel CreateDocumentConfigurationResponseModel();

    DocumentTypeConfigurationResponseModel CreateDocumentTypeConfigurationResponseModel()
        => throw new NotImplementedException();

    MemberConfigurationResponseModel CreateMemberConfigurationResponseModel();

    MemberTypeConfigurationResponseModel CreateMemberTypeConfigurationResponseModel()
        => throw new NotImplementedException();

    MediaConfigurationResponseModel CreateMediaConfigurationResponseModel();

    MediaTypeConfigurationResponseModel CreateMediaTypeConfigurationResponseModel()
        => throw new NotImplementedException();
}
