using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Umbraco.Cms.Api.Management.OpenApi;

internal class ReponseHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        foreach ((var key, OpenApiResponse? value) in operation.Responses)
        {
            switch (int.Parse(key))
            {
                case StatusCodes.Status201Created:
                    SetHeader(value, "Location", "Location of the newly created resource", "string", "uri");
                    break;
            }
        }
    }

    private void SetHeader(OpenApiResponse value, string headerName, string description, string type, string format)
    {

        if (value.Headers is null)
        {
            value.Headers = new Dictionary<string, OpenApiHeader>();
        }

        value.Headers[headerName] = new OpenApiHeader()
        {
            Description = description,
            Schema = new OpenApiSchema { Description = description, Type = type, Format = format }
        };
    }
}
