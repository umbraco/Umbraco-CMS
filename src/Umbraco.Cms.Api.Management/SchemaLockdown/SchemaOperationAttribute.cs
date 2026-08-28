using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Api.Management.SchemaLockdown;

/// <summary>
/// Declares the operation an action performs when its HTTP verb misrepresents it.
/// </summary>
/// <remarks>
/// Applied to endpoints such as the composition pickers, which are POST because the query needs a request body but
/// never write anything.
/// <para>
/// Naming an entity type declares the operation for that one alone, which an action doing different things to each
/// of the entity types its controller declares needs: creating a template for a document type creates the template
/// and updates the document type. A declaration naming no entity type covers every type that has none of its own,
/// and a type with neither falls back to the HTTP verb.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class SchemaOperationAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaOperationAttribute"/> class, for every entity type the
    /// controller declares.
    /// </summary>
    /// <param name="operation">The operation the action performs.</param>
    public SchemaOperationAttribute(SchemaOperation operation) => Operation = operation;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaOperationAttribute"/> class, for one entity type.
    /// </summary>
    /// <param name="entityType">The entity type the operation applies to.</param>
    /// <param name="operation">The operation the action performs on it.</param>
    public SchemaOperationAttribute(string entityType, SchemaOperation operation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        EntityType = entityType;
        Operation = operation;
    }

    /// <summary>
    /// Gets the operation the action performs.
    /// </summary>
    public SchemaOperation Operation { get; }

    /// <summary>
    /// Gets the entity type the operation applies to, or null when it applies to all of them.
    /// </summary>
    public string? EntityType { get; }
}
