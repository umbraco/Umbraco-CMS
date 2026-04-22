using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_16_3_0;

/// <summary>
/// Represents a migration that adds rich text editor capabilities to the system as part of the upgrade to version 16.3.0.
/// </summary>
[Obsolete("Remove in Umbraco 18.")]
public class AddRichTextEditorCapabilities : AsyncMigrationBase
{
    private readonly IDataTypeService _dataTypeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddRichTextEditorCapabilities"/> class, used to add capabilities to the rich text editor during the migration process.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> for the migration operation.</param>
    /// <param name="dataTypeService">The <see cref="IDataTypeService"/> used to manage data types.</param>
    public AddRichTextEditorCapabilities(IMigrationContext context, IDataTypeService dataTypeService)
        : base(context)
    {
        _dataTypeService = dataTypeService;
    }

    protected override async Task MigrateAsync()
    {
        IEnumerable<IDataType> dataTypes = await _dataTypeService.GetByEditorUiAlias("Umb.PropertyEditorUi.Tiptap");

        foreach (IDataType dataType in dataTypes)
        {
            HashSet<string> extensions = new();

            if (dataType.ConfigurationData.TryGetValue("extensions", out var tmp) && tmp is List<string> existing)
            {
                extensions.UnionWith(existing);
            }

            string[] newExtensions =
            [
                "Umb.Tiptap.RichTextEssentials",
                "Umb.Tiptap.Anchor",
                "Umb.Tiptap.Block",
                "Umb.Tiptap.Blockquote",
                "Umb.Tiptap.Bold",
                "Umb.Tiptap.BulletList",
                "Umb.Tiptap.CodeBlock",
                "Umb.Tiptap.Heading",
                "Umb.Tiptap.HorizontalRule",
                "Umb.Tiptap.HtmlAttributeClass",
                "Umb.Tiptap.HtmlAttributeDataset",
                "Umb.Tiptap.HtmlAttributeId",
                "Umb.Tiptap.HtmlAttributeStyle",
                "Umb.Tiptap.HtmlTagDiv",
                "Umb.Tiptap.HtmlTagSpan",
                "Umb.Tiptap.Italic",
                "Umb.Tiptap.OrderedList",
                "Umb.Tiptap.Strike",
                "Umb.Tiptap.TrailingNode",
            ];

            extensions.UnionWith(newExtensions);

            dataType.ConfigurationData["extensions"] = extensions.ToArray();

            _ = await _dataTypeService.UpdateAsync(dataType, Constants.Security.SuperUserKey);
        }
    }
}
