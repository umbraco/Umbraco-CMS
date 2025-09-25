using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0.SingleBlockList;

[Obsolete("Will be removed in V22")] // Available in v17, activated in v18. Migration needs to work on LTS to LTS 17=>21
public interface ITypedSingleBlockListProcessor
{
    public Type PropertyEditorValueType { get; }

    public IEnumerable<string> PropertyEditorAliases { get; }

    /// <summary>
    /// object?: the editorValue being processed
    /// Func<object?, bool>: the function that will be called when nested content is detected
    /// Func<BlockListValue, SingleBlockValue>: the function that will do the migration
    /// </summary>
    public Func<object?, Func<object?, bool>, Func<BlockListValue, SingleBlockValue>, bool> Process { get; }
}
