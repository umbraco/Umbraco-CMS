using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Api.Common.Rendering;
using Umbraco.Cms.Core.DeliveryApi;

namespace Umbraco.Cms.Api.Delivery.Rendering;

internal sealed class RequestContextOutputExpansionStrategyV2 : ElementOnlyOutputExpansionStrategy, IOutputExpansionStrategy
{
    private readonly ILogger<RequestContextOutputExpansionStrategyV2> _logger;

    public RequestContextOutputExpansionStrategyV2(
        IHttpContextAccessor httpContextAccessor,
        IApiPropertyRenderer propertyRenderer,
        ILogger<RequestContextOutputExpansionStrategyV2> logger)
        : base(propertyRenderer)
    {
        _logger = logger;

        InitializeExpandAndInclude(httpContextAccessor);
    }

    private void InitializeExpandAndInclude(IHttpContextAccessor httpContextAccessor)
    {
        string? QueryValue(string key) => httpContextAccessor.HttpContext?.Request.Query[key];

        var toExpand = QueryValue(ExpandParameterName) ?? None;
        var toInclude = QueryValue(FieldsParameterName) ?? All;

        try
        {
            ExpandProperties.Push(Node.Parse(toExpand));
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, $"Could not parse the '{ExpandParameterName}' parameter. See exception for details.");
            throw new ArgumentException($"Could not parse the '{ExpandParameterName}' parameter: {ex.Message}");
        }

        try
        {
            IncludeProperties.Push(Node.Parse(toInclude));
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, $"Could not parse the '{FieldsParameterName}' parameter. See exception for details.");
            throw new ArgumentException($"Could not parse the '{FieldsParameterName}' parameter: {ex.Message}");
        }
    }
}
