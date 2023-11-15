using System.Text.Json;
using System.Text.Json.Nodes;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
/// A contextual permission with optional granularity
/// </summary>
public struct ContextualPermission
{
    public ContextualPermission()
    {
        Identifier = null;
    }

    public ContextualPermission(string context, string permission)
    {
        Context = context;
        Permission = permission;
    }

    public ContextualPermission(string context, string? identifier, string permission)
    {
        Context = context;
        Permission = permission;
        Identifier = identifier;
    }

    /// <summary>
    /// The context in which the Identifier and permission makes sense
    /// </summary>
    /// <example>
    /// umb-node, umb-document, umb-media, ...
    /// </example>
    /// <remarks>
    /// Implementers are encouraged to prefix their contexts with a unique slug to avoid collisions
    /// </remarks> // todo granular permissions: bellisima global permissions context name
    public string Context { get; set; } = string.Empty;

    /// <summary>
    /// Data that specifies a subset of the given context. If null, the permission targets the whole context.
    /// </summary>
    /// <remarks>
    /// For now will be Guid[] to be able to specify node-ids
    /// </remarks>
    public string? Identifier { get; set; }

    /// <summary>
    /// The string representation of the permission within the defined Context.
    /// </summary>
    /// <example>
    /// Umbraco examples: browse, read, write, ...
    /// </example>
    public string Permission { get; set; } = string.Empty;
}
