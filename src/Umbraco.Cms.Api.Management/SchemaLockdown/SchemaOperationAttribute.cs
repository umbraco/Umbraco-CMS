using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Api.Management.SchemaLockdown;

/// <summary>
/// Declares the operation an action performs when its HTTP verb doesn't match it, for example a POST that only reads.
/// </summary>
/// <remarks>
/// Name an entity type when the action does different things to each one: creating a template for a document type
/// creates the template and updates the document type. Entity types without a declaration fall back to the HTTP verb.
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
