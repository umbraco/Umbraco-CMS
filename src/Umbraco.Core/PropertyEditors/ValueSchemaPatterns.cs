namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Contains regex patterns used in JSON Schema validation for property editor values.
/// </summary>
public static class ValueSchemaPatterns
{
    /// <summary>
    /// Regex pattern for validating UUID/GUID strings.
    /// </summary>
    /// <remarks>
    /// Matches UUIDs with or without hyphens, case-insensitive (lowercase in pattern).
    /// Examples: "550e8400-e29b-41d4-a716-446655440000" or "550e8400e29b41d4a716446655440000"
    /// </remarks>
    public const string Uuid = "^[0-9a-f]{8}-?[0-9a-f]{4}-?[0-9a-f]{4}-?[0-9a-f]{4}-?[0-9a-f]{12}$";
}
