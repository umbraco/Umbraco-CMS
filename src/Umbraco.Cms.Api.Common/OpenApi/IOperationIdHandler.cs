using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Umbraco.Cms.Api.Common.OpenApi;

public interface IOperationIdHandler
{
     bool CanHandle(ApiDescription apiDescription);

     string Handle(ApiDescription apiDescription);
}
