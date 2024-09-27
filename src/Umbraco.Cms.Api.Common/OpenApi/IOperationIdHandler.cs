using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Umbraco.Cms.Api.Common.OpenApi;

public interface IOperationIdHandler
{
     [Obsolete("Use CanHandle(ApiDescription apiDescription, string documentName) instead. Will be removed in v16.")]
     bool CanHandle(ApiDescription apiDescription);

#pragma warning disable CS0618 // Type or member is obsolete
     bool CanHandle(ApiDescription apiDescription, string documentName) => CanHandle(apiDescription);
#pragma warning restore CS0618 // Type or member is obsolete

     string Handle(ApiDescription apiDescription);
}
