using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Api.Management.SchemaLockdown;

/// <summary>
/// Blocks requests that would change schema which the current configuration marks read-only.
/// </summary>
internal sealed class SchemaLockdownFilter : IActionFilter
{
    private readonly ISchemaLockdownMatrixAccessor _matrixAccessor;
    private readonly string _entityType;
    private readonly SchemaOperation _operation;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaLockdownFilter"/> class.
    /// </summary>
    /// <param name="matrixAccessor">Provides the frozen decision matrix.</param>
    /// <param name="entityType">The entity type the action's controller manages.</param>
    /// <param name="operation">The operation the action performs.</param>
    public SchemaLockdownFilter(
        ISchemaLockdownMatrixAccessor matrixAccessor,
        string entityType,
        SchemaOperation operation)
    {
        _matrixAccessor = matrixAccessor;
        _entityType = entityType;
        _operation = operation;
    }

    /// <inheritdoc />
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (_matrixAccessor.Matrix.IsAllowed(_entityType, _operation))
        {
            return;
        }

        var problemDetails = new ProblemDetails
        {
            Title = "Schema is read-only",
            Detail = $"Changing {_entityType} is not permitted because schema lockdown is enabled.",
            Status = StatusCodes.Status403Forbidden,
            Type = "Error",
        };

        context.Result = new ObjectResult(problemDetails) { StatusCode = StatusCodes.Status403Forbidden };
    }

    /// <inheritdoc />
    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
