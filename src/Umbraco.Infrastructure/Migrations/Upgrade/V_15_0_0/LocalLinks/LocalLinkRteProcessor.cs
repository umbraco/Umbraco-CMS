using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0.LocalLinks;

/// <summary>
/// Handles the processing and migration of local links within Rich Text Editor (RTE) content as part of the upgrade process to Umbraco version 15.0.0.
/// </summary>
[Obsolete("Scheduled for removal in Umbraco 18.")]
public class LocalLinkRteProcessor : ITypedLocalLinkProcessor
{
    /// <summary>
    /// Gets the type of the property editor value, which is <see cref="RichTextEditorValue"/>.
    /// </summary>
    public Type PropertyEditorValueType => typeof(RichTextEditorValue);

    /// <summary>
    /// Gets the aliases of the property editors that this processor supports.
    /// </summary>
    public IEnumerable<string> PropertyEditorAliases =>
    [
        "Umbraco.TinyMCE", Constants.PropertyEditors.Aliases.RichText
    ];

    /// <summary>
    /// Gets a delegate that processes rich text editor content for local links during migration.
    /// The delegate takes an input object (representing the content), a predicate to filter objects, and a string transformation function to modify link values.
    /// Returns <c>true</c> if processing succeeds; otherwise, <c>false</c>.
    /// </summary>
    public Func<object?, Func<object?, bool>, Func<string, string>, bool> Process => ProcessRichText;

    /// <summary>
    /// Processes a rich text editor value by updating its markup and any nested block values using the provided processing functions.
    /// </summary>
    /// <param name="value">The value to process. This should be a <see cref="RichTextEditorValue"/>; if not, the method returns <c>false</c> and does nothing.</param>
    /// <param name="processNested">A function that processes nested property values within each block. If this function returns <c>true</c> for any nested value, the method considers the content changed.</param>
    /// <param name="processStringValue">A function that processes and potentially transforms the markup string of the rich text editor value.</param>
    /// <returns><c>true</c> if any changes were made to the markup or nested block values; otherwise, <c>false</c>.</returns>
    public bool ProcessRichText(
        object? value,
        Func<object?, bool> processNested,
        Func<string, string> processStringValue)
    {
        if (value is not RichTextEditorValue richTextValue)
        {
            return false;
        }

        bool hasChanged = false;

        var newMarkup = processStringValue.Invoke(richTextValue.Markup);

        // fix recursive hickup in ConvertRichTextEditorProperties
        newMarkup = RteBlockHelper.ConvertBlockUdisToKeys(newMarkup);

        if (newMarkup.Equals(richTextValue.Markup) == false)
        {
            hasChanged = true;
            richTextValue.Markup = newMarkup;
        }

        if (richTextValue.Blocks is null)
        {
            return hasChanged;
        }

        foreach (BlockItemData blockItemData in richTextValue.Blocks.ContentData)
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
