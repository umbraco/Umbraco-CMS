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
    /// <para><c>object?</c>: the editorValue being processed.</para>
    /// <para><c>Func&lt;object?, bool&gt;</c>: the function that will be called when nested content is detected.</para>
    /// </summary>
    public Func<object?, Func<object?, bool>, Func<BlockListValue, object>, bool> Process { get; }
}
