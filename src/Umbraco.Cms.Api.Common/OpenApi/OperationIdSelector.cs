using Asp.Versioning;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Umbraco.Cms.Api.Common.OpenApi;

public class OperationIdSelector : IOperationIdSelector
{
    private readonly IEnumerable<IOperationIdHandler> _operationIdHandlers;

    [Obsolete("Use non-obsolete constructor. This will be removed in Umbraco 15.")]
    public OperationIdSelector()
        : this(Enumerable.Empty<IOperationIdHandler>())
    {
    }

    public OperationIdSelector(IEnumerable<IOperationIdHandler> operationIdHandlers)
        => _operationIdHandlers = operationIdHandlers;

    [Obsolete("Use overload that only takes ApiDescription instead. This will be removed in Umbraco 15.")]
    public virtual string? OperationId(ApiDescription apiDescription, ApiVersioningOptions apiVersioningOptions) => OperationId(apiDescription);

    public virtual string? OperationId(ApiDescription apiDescription)
    {
        IOperationIdHandler? handler = _operationIdHandlers.FirstOrDefault(h => h.CanHandle(apiDescription));
        return handler?.Handle(apiDescription);
    }
}
