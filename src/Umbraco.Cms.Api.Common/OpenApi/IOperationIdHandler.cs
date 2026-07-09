using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
///     Defines a handler for generating OpenAPI operation IDs.
/// </summary>
public interface IOperationIdHandler
{
    /// <summary>
    ///     Determines whether this handler can generate an operation ID for the specified API description.
    /// </summary>
    /// <param name="apiDescription">The API description to check.</param>
    /// <returns><c>true</c> if this handler can handle the API description; otherwise, <c>false</c>.</returns>
    bool CanHandle(ApiDescription apiDescription);

    /// <summary>
    ///     Generates an operation ID for the specified API description.
    /// </summary>
    /// <param name="apiDescription">The API description to generate an operation ID for.</param>
    /// <returns>The generated operation ID.</returns>
    string Handle(ApiDescription apiDescription);
}
