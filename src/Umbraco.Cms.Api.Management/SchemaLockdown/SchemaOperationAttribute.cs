using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Api.Management.SchemaLockdown;

/// <summary>
/// Declares the operation an action performs when its HTTP verb misrepresents it.
/// </summary>
/// <remarks>
/// Applied to endpoints such as the composition pickers, which are POST because the query needs a request body
/// but never write anything.
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public sealed class SchemaOperationAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaOperationAttribute"/> class.
    /// </summary>
    /// <param name="operation">The operation the action performs.</param>
    public SchemaOperationAttribute(SchemaOperation operation) => Operation = operation;

    /// <summary>
    /// Gets the operation the action performs.
    /// </summary>
    public SchemaOperation Operation { get; }
}
