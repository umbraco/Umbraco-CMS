using Asp.Versioning;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Umbraco.Cms.Api.Common.Configuration;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
/// Determines whether an API description should be included in a specific documentation set based on the document name
/// and API metadata.
/// </summary>
public class DocumentInclusionSelector : IDocumentInclusionSelector
{
    /// <inheritdoc/>
    public bool Include(string documentName, ApiDescription apiDescription)
    {
        if (apiDescription.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor
            && controllerActionDescriptor.HasMapToApiAttribute(documentName))
        {
            return true;
        }

        ApiVersionMetadata apiVersionMetadata = apiDescription.ActionDescriptor.GetApiVersionMetadata();
        return apiVersionMetadata.Name == documentName
               || (string.IsNullOrEmpty(apiVersionMetadata.Name) && documentName == DefaultApiConfiguration.ApiName);
    }
}

