namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0.LocalLinks;

/// <summary>
/// Represents a service that processes typed local links as part of the Umbraco upgrade to version 15.0.0.
/// Implementations of this interface handle the transformation or validation of local links during migration.
/// </summary>
[Obsolete("Scheduled for removal in Umbraco 18.")]
public interface ITypedLocalLinkProcessor
{
    /// <summary>
    /// Gets the <see cref="Type"/> that represents the value type expected by the property editor
    /// when processing local links.
    /// </summary>
    public Type PropertyEditorValueType { get; }

    /// <summary>
    /// Gets the collection of property editor aliases supported by this local link processor.
    /// </summary>
    public IEnumerable<string> PropertyEditorAliases { get; }

    /// <summary>
    /// Gets a function that processes a typed local link.
    /// The returned function takes an object representing the link, a predicate to validate the link, and a transformation function for the link string, returning true if the link was processed successfully.
    /// </summary>
    public Func<object?, Func<object?, bool>, Func<string, string>, bool> Process { get; }
}
