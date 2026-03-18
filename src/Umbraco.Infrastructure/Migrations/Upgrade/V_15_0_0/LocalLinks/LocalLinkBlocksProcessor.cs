using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0.LocalLinks;

/// <summary>
/// Handles the processing of local link blocks as part of the upgrade process to Umbraco version 15.0.0.
/// </summary>
[Obsolete("Scheduled for removal in Umbraco 18.")]
public abstract class LocalLinkBlocksProcessor
{
    /// <summary>
    /// Iterates through the blocks contained in the specified <paramref name="value"/> (expected to be a <c>BlockValue</c>),
    /// and applies the <paramref name="processNested"/> function to each nested property value.
    /// </summary>
    /// <param name="value">The value to process, which should be a <c>BlockValue</c> containing block items.</param>
    /// <param name="processNested">A function that processes each nested property value within the blocks. Should return <c>true</c> if the value was changed.</param>
    /// <param name="processStringValue">A function to process string values within the blocks. (Note: This parameter is not used in the current implementation.)</param>
    /// <returns><c>true</c> if any nested property value was changed during processing; otherwise, <c>false</c>.</returns>
    public bool ProcessBlocks(
        object? value,
        Func<object?, bool> processNested,
        Func<string, string> processStringValue)
    {
        if (value is not BlockValue blockValue)
        {
            return false;
        }

        bool hasChanged = false;

        foreach (BlockItemData blockItemData in blockValue.ContentData)
        {
            foreach (BlockPropertyValue blockPropertyValue in blockItemData.Values)
            {
                if (processNested.Invoke(blockPropertyValue.Value))
                {
                    hasChanged = true;
                }
            }
        }

        return hasChanged;
    }
}

/// <summary>
/// Handles the processing and migration of local link block lists as part of the upgrade process to Umbraco version 15.0.0.
/// </summary>
[Obsolete("Scheduled for removal in Umbraco 18.")]
public class LocalLinkBlockListProcessor : LocalLinkBlocksProcessor, ITypedLocalLinkProcessor
{
    /// <summary>
    /// Gets the type of the property editor value, which is always <see cref="BlockListValue"/>.
    /// </summary>
    public Type PropertyEditorValueType => typeof(BlockListValue);

    /// <summary>
    /// Gets the collection of property editor aliases that this processor supports for processing block list properties.
    /// </summary>
    public IEnumerable<string> PropertyEditorAliases => [Constants.PropertyEditors.Aliases.BlockList];

    /// <summary>
    /// Gets a function that processes local link blocks within a block list.
    /// </summary>
    /// <returns>
    /// A function that takes an object representing the block list, a predicate to determine if a block should be processed, and a string transformation function, returning true if processing was successful.
    /// </returns>
    public Func<object?, Func<object?, bool>, Func<string, string>, bool> Process => ProcessBlocks;
}

/// <summary>
/// Handles the processing of local link block grids as part of the upgrade process to Umbraco version 15.0.0.
/// </summary>
[Obsolete("Scheduled for removal in Umbraco 18.")]
public class LocalLinkBlockGridProcessor : LocalLinkBlocksProcessor, ITypedLocalLinkProcessor
{
    /// <summary>
    /// Gets the <see cref="Type"/> representing the value type used by the property editor, which is <see cref="BlockGridValue"/>.
    /// </summary>
    public Type PropertyEditorValueType => typeof(BlockGridValue);

    /// <summary>
    /// Gets the property editor aliases handled by this block grid processor.
    /// </summary>
    public IEnumerable<string> PropertyEditorAliases => [Constants.PropertyEditors.Aliases.BlockGrid];

    /// <summary>
    /// Gets a delegate that processes block grid data containing local links.
    /// The delegate accepts:
    /// <para>- An object representing the block grid data to process.</para>
    /// <para>- A predicate function to determine if a given object should be processed.</para>
    /// <para>- A string transformation function to apply to local link values.</para>
    /// Returns <c>true</c> if processing succeeds; otherwise, <c>false</c>.
    /// </summary>
    public Func<object?, Func<object?, bool>, Func<string, string>, bool> Process => ProcessBlocks;
}
