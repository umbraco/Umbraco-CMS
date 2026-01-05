using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0.SingleBlockList;

/// <summary>
/// Defines a processor for handling typed single block list property editor values during migration.
/// </summary>
[Obsolete("Will be removed in V22")] // Available in v17, activated in v18. Migration needs to work on LTS to LTS 17=>21
public interface ITypedSingleBlockListProcessor
{
    /// <summary>
    /// The type of the propertyEditor expects to receive as a value to process
    /// </summary>
    public Type PropertyEditorValueType { get; }

    /// <summary>
    /// The property (data)editor aliases that this processor supports, as defined on their DataEditor attributes
    /// </summary>
    public IEnumerable<string> PropertyEditorAliases { get; }

    /// <summary>
    /// Gets the function that processes property editor values during migration and detects nested content structures.
    /// <para><c>object?</c>: the editor value being processed.</para>
    /// <para><c>Func&lt;object?, bool&gt;</c>: the callback that is invoked when nested content is detected in a processed value.</para>
    /// <para><c>Func&lt;BlockListValue, object&gt;</c>: the function used to convert a <see cref="BlockListValue"/> into the nested content value passed to the callback.</para>
    /// <para><c>bool</c>: returns <see langword="true"/> if the value was successfully processed and any nested content was handled; otherwise, <see langword="false"/>.</para>
    /// </summary>
    public Func<object?, Func<object?, bool>, Func<BlockListValue, object>, bool> Process { get; }
}
