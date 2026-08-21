using Microsoft.Extensions.Configuration;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
/// Configures which schema entity types are read-only through the Management API.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigSchemaLockdown)]
public class SchemaLockdownSettings
{
    /// <summary>
    /// Gets or sets a value indicating whether schema lockdown is active. Defaults to <c>false</c>.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets the entity types that are governed while <see cref="Enabled"/> is <c>true</c>.
    /// </summary>
    /// <remarks>
    /// Configuration binding is append-only, so values listed here are added to the defaults and an entity type
    /// cannot be removed. Use <see cref="Enabled"/> to turn the feature off entirely.
    /// </remarks>
    public ISet<string> LockedEntityTypes { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Constants.UdiEntityType.DocumentType,
        Constants.UdiEntityType.MediaType,
        Constants.UdiEntityType.MemberType,
        Constants.UdiEntityType.DataType,
        Constants.UdiEntityType.Script,
        Constants.UdiEntityType.Stylesheet,
        Constants.UdiEntityType.DictionaryItem,
    };

    /// <summary>
    /// Throws when a configured entity type is not one schema lockdown governs.
    /// </summary>
    /// <remarks>
    /// Any string binds, so a misspelled entity type is carried into the settings and then never governs anything,
    /// leaving it unlocked without any error. Each configured value is therefore checked here against the governed
    /// set, independently of what was bound — checking against the bound set instead would pass for any value that
    /// happens to collide with a default.
    /// </remarks>
    /// <param name="section">The configuration section the settings were bound from.</param>
    internal static void ValidateBinding(IConfigurationSection section)
    {
        var unrecognised = section.GetSection(nameof(LockedEntityTypes))
            .GetChildren()
            .Select(child => child.Value)
            .Where(value => SchemaEntityTypes.TryResolve(value, out _) is false)
            .ToArray();

        if (unrecognised.Length > 0)
        {
            throw new InvalidOperationException(
                $"Unrecognised schema lockdown entity type(s): {string.Join(", ", unrecognised)}. "
                + $"Valid values are: {string.Join(", ", SchemaEntityTypes.All)}.");
        }
    }
}
