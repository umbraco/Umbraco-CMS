using Asp.Versioning;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
///     Defines a selector for choosing operation IDs from registered handlers.
/// </summary>
public interface IOperationIdSelector
{
    /// <summary>
    ///     Selects an operation ID for the specified API description.
    /// </summary>
    /// <param name="apiDescription">The API description to generate an operation ID for.</param>
    /// <returns>The operation ID, or <c>null</c> if none could be determined.</returns>
    string? OperationId(ApiDescription apiDescription);
}
