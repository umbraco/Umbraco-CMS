﻿using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Api.Management.ViewModels.Member;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IConfigurationPresentationFactory
{
    DocumentConfigurationResponseModel CreateDocumentConfigurationModel();
    MemberConfigurationResponseModel CreateMemberConfigurationResponseModel();
    MediaConfigurationResponseModel CreateMediaConfigurationResponseModel();
}
