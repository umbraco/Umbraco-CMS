using Asp.Versioning;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
///     Selects an operation ID for an API description using registered handlers.
/// </summary>
public class OperationIdSelector : IOperationIdSelector
{
    private readonly IEnumerable<IOperationIdHandler> _operationIdHandlers;

    /// <summary>
    ///     Initializes a new instance of the <see cref="OperationIdSelector"/> class.
    /// </summary>
    [Obsolete("Use non-obsolete constructor. This will be removed in Umbraco 15.")]
    public OperationIdSelector()
        : this(Enumerable.Empty<IOperationIdHandler>())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="OperationIdSelector"/> class.
    /// </summary>
    /// <param name="operationIdHandlers">The registered operation ID handlers.</param>
    public OperationIdSelector(IEnumerable<IOperationIdHandler> operationIdHandlers)
        => _operationIdHandlers = operationIdHandlers;

    /// <inheritdoc/>
    public virtual string? OperationId(ApiDescription apiDescription)
    {
        IOperationIdHandler? handler = _operationIdHandlers.FirstOrDefault(h => h.CanHandle(apiDescription));
        return handler?.Handle(apiDescription);
    }
}
