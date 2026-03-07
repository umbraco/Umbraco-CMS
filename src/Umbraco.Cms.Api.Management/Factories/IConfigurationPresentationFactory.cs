using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Api.Management.ViewModels.MemberType;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Represents a factory responsible for creating configuration presentation models used in the management API.
/// </summary>
public interface IConfigurationPresentationFactory
{
    /// <summary>
    /// Creates and returns a new <see cref="DocumentConfigurationResponseModel"/> instance.
    /// </summary>
    /// <returns>The created <see cref="DocumentConfigurationResponseModel"/>.</returns>
    DocumentConfigurationResponseModel CreateDocumentConfigurationResponseModel();

    /// <summary>
    /// Creates a new instance of <see cref="Umbraco.Cms.Api.Management.Models.DocumentTypeConfigurationResponseModel"/>.
    /// </summary>
    /// <returns>A <see cref="Umbraco.Cms.Api.Management.Models.DocumentTypeConfigurationResponseModel"/> representing the document type configuration.</returns>
    DocumentTypeConfigurationResponseModel CreateDocumentTypeConfigurationResponseModel()
        => throw new NotImplementedException();

    /// <summary>
    /// Creates a response model representing the member configuration.
    /// </summary>
    /// <returns>A <see cref="Umbraco.Cms.Api.Management.Models.MemberConfigurationResponseModel"/> instance.</returns>
    MemberConfigurationResponseModel CreateMemberConfigurationResponseModel();

    /// <summary>
    /// Creates and returns a new <see cref="Umbraco.Cms.Api.Management.Models.MemberTypeConfigurationResponseModel"/> instance.
    /// </summary>
    /// <returns>The created <see cref="Umbraco.Cms.Api.Management.Models.MemberTypeConfigurationResponseModel"/>.</returns>
    MemberTypeConfigurationResponseModel CreateMemberTypeConfigurationResponseModel()
        => throw new NotImplementedException();

    /// <summary>
    /// Creates a media configuration response model.
    /// </summary>
    /// <returns>A <see cref="Umbraco.Cms.Api.Management.Models.MediaConfigurationResponseModel"/> representing the media configuration.</returns>
    [Obsolete("No longer used. Scheduled for removal in Umbraco 18.")]
    MediaConfigurationResponseModel CreateMediaConfigurationResponseModel();

    /// <summary>
    /// Creates a <see cref="Umbraco.Cms.Api.Management.Models.MediaTypeConfigurationResponseModel"/> representing the media type configuration.
    /// </summary>
    /// <returns>The created <see cref="Umbraco.Cms.Api.Management.Models.MediaTypeConfigurationResponseModel"/>.</returns>
    MediaTypeConfigurationResponseModel CreateMediaTypeConfigurationResponseModel()
        => throw new NotImplementedException();
}
