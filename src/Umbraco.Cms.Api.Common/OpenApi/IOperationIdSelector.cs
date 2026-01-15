using Asp.Versioning;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Umbraco.Cms.Api.Common.OpenApi;

public interface IOperationIdSelector
{
    string? OperationId(ApiDescription apiDescription);
}
