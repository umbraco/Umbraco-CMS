using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
/// Defines a method that determines whether a given API description should be included in a specific documentation
/// document.
/// </summary>
public interface IDocumentInclusionSelector
{
    /// <summary>
    /// Determines whether the specified API description should be included in the generated documentation for the given
    /// document name.
    /// </summary>
    /// <param name="documentName">The name of the documentation document being generated.</param>
    /// <param name="apiDescription">The API description to evaluate for inclusion.</param>
    /// <returns>true if the API description should be included in the documentation; otherwise, false.</returns>
    bool Include(string documentName, ApiDescription apiDescription);
}
