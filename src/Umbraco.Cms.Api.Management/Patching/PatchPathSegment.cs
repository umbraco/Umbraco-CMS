namespace Umbraco.Cms.Api.Management.Patching;

/// <summary>
/// Represents a single segment in a patch path expression.
/// </summary>
public abstract record PatchPathSegment;

/// <summary>
/// Accesses a named property on an object (e.g., /name, /variants).
/// </summary>
public sealed record PropertySegment(string Name) : PatchPathSegment;

/// <summary>
/// Filters an array by matching element properties (e.g., [culture=en-US,segment=null]).
/// All conditions must match (AND logic).
/// </summary>
public sealed record FilterSegment(FilterCondition[] Conditions) : PatchPathSegment;

/// <summary>
/// Accesses an array element by numeric index (e.g., /0, /1).
/// </summary>
public sealed record IndexSegment(int Index) : PatchPathSegment;

/// <summary>
/// Marks the end-of-array position for Add operations (/-).
/// </summary>
public sealed record AppendSegment : PatchPathSegment;

/// <summary>
/// A single filter condition: key=value where value can be null.
/// </summary>
public sealed record FilterCondition(string Key, string? Value);
