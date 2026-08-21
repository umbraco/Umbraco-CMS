using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Api.Management.SchemaLockdown;

/// <summary>
/// Attaches <see cref="SchemaLockdownFilter"/> to every action on a controller carrying <see cref="EntityTypeAttribute"/>.
/// </summary>
/// <remarks>
/// Resolving the operation once per action at start-up keeps the request path to a dictionary lookup, and means a
/// newly added mutating endpoint is governed without anyone having to remember to tag it.
/// </remarks>
internal class SchemaLockdownConvention : IControllerModelConvention
{
    private readonly ISchemaLockdownMatrixAccessor _matrixAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaLockdownConvention"/> class.
    /// </summary>
    /// <param name="matrixAccessor">Provides the frozen decision matrix.</param>
    public SchemaLockdownConvention(ISchemaLockdownMatrixAccessor matrixAccessor)
        => _matrixAccessor = matrixAccessor;

    /// <inheritdoc />
    public void Apply(ControllerModel controller)
    {
        EntityTypeAttribute? entityType = controller.Attributes.OfType<EntityTypeAttribute>().FirstOrDefault();

        if (entityType is null)
        {
            return;
        }

        foreach (ActionModel action in controller.Actions)
        {
            SchemaOperationAttribute? declared = action.Attributes.OfType<SchemaOperationAttribute>().FirstOrDefault();
            var httpMethods = action.Selectors
                .SelectMany(selector => selector.ActionConstraints?.OfType<HttpMethodActionConstraint>() ?? [])
                .SelectMany(constraint => constraint.HttpMethods)
                .Distinct()
                .ToArray();

            SchemaOperation operation = SchemaOperationResolver.Resolve(httpMethods, declared);

            if (operation == SchemaOperation.Read)
            {
                continue;
            }

            action.Filters.Add(new SchemaLockdownFilter(_matrixAccessor, entityType.EntityType, operation));
        }
    }
}
