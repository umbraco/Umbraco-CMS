using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_16_0_0;

public class MigrateTinyMceToTiptap : AsyncMigrationBase
{
    private readonly IDataTypeService _dataTypeService;
    private readonly TinyMceToTiptapMigrationSettings _options;

    public MigrateTinyMceToTiptap(IMigrationContext context, IDataTypeService dataTypeService, IOptions<TinyMceToTiptapMigrationSettings> options) : base(context)
    {
        _dataTypeService = dataTypeService;
        _options = options.Value;
    }

    protected override async Task MigrateAsync()
    {
        if (_options.DisableMigration)
        {
            return;
        }

        IEnumerable<IDataType> dataTypes = await _dataTypeService.GetByEditorUiAlias("Umb.PropertyEditorUi.TinyMCE");

        foreach (IDataType dataType in dataTypes)
        {
            MigrateToTipTap(dataType);
            await _dataTypeService.UpdateAsync(dataType, Constants.Security.SuperUserKey);
        }
    }

    private void MigrateToTipTap(IDataType dataType)
    {
        dataType.EditorUiAlias = "Umb.PropertyEditorUi.Tiptap";

        if (!dataType.ConfigurationData.TryGetValue("toolbar", out var toolbar) || toolbar is not List<string> toolBarList)
        {
            return;
        }

        dataType.ConfigurationData.Remove("mode");
        dataType.ConfigurationData.Remove("hideLabel");

        var newToolbar = toolBarList.Select(MapToolbarItem).WhereNotNull().ToList();
        dataType.ConfigurationData["toolbar"] = new List<List<List<string>>> { new() { newToolbar } };

        var extensions = new List<string>
        {
            "Umb.Tiptap.RichTextEssentials",
            "Umb.Tiptap.Embed",
            "Umb.Tiptap.Figure",
            "Umb.Tiptap.Image",
            "Umb.Tiptap.Link",
            "Umb.Tiptap.MediaUpload",
            "Umb.Tiptap.Subscript",
            "Umb.Tiptap.Superscript",
            "Umb.Tiptap.Table",
            "Umb.Tiptap.TextAlign",
            "Umb.Tiptap.TextDirection",
            "Umb.Tiptap.TextIndent",
            "Umb.Tiptap.Underline"
        };

        // If the rich text has block enabled, we also need to add the block extension
        if (dataType.ConfigurationObject is RichTextConfiguration richTextConfiguration)
        {
            if (richTextConfiguration.Blocks?.Any() ?? false)
            {
                extensions.Add("Umb.Tiptap.Block");
            }
        }

        dataType.ConfigurationData["extensions"] = extensions.ToArray();
    }

    private string? MapToolbarItem(string item)
    {
        return item switch
        {
            "undo" => "Umb.Tiptap.Toolbar.Undo",
            "redo" => "Umb.Tiptap.Toolbar.Redo",
            "cut" => null,
            "copy" => null,
            "paste" => null,
            "styles" => "Umb.Tiptap.Toolbar.StyleSelect",
            "fontname" => "Umb.Tiptap.Toolbar.FontFamily",
            "fontfamily" => "Umb.Tiptap.Toolbar.FontFamily",
            "fontsize" => "Umb.Tiptap.Toolbar.FontSize",
            "forecolor" => "Umb.Tiptap.Toolbar.TextColorForeground",
            "backcolor" => "Umb.Tiptap.Toolbar.TextColorBackground",
            "blockquote" => "Umb.Tiptap.Toolbar.Blockquote",
            "formatblock" => null,
            "removeformat" => "Umb.Tiptap.Toolbar.ClearFormatting",
            "bold" => "Umb.Tiptap.Toolbar.Bold",
            "italic" => "Umb.Tiptap.Toolbar.Italic",
            "underline" => "Umb.Tiptap.Toolbar.Underline",
            "strikethrough" => "Umb.Tiptap.Toolbar.Strike",
            "alignleft" => "Umb.Tiptap.Toolbar.TextAlignLeft",
            "aligncenter" => "Umb.Tiptap.Toolbar.TextAlignCenter",
            "alignright" => "Umb.Tiptap.Toolbar.TextAlignRight",
            "alignjustify" => "Umb.Tiptap.Toolbar.TextAlignJustify",
            "bullist" => "Umb.Tiptap.Toolbar.BulletList",
            "numlist" => "Umb.Tiptap.Toolbar.OrderedList",
            "outdent" => "Umb.Tiptap.Toolbar.TextOutdent",
            "indent" => "Umb.Tiptap.Toolbar.TextIndent",
            "anchor" => "Umb.Tiptap.Toolbar.Anchor",
            "table" => "Umb.Tiptap.Toolbar.Table",
            "hr" => "Umb.Tiptap.Toolbar.HorizontalRule",
            "subscript" => "Umb.Tiptap.Toolbar.Subscript",
            "superscript" => "Umb.Tiptap.Toolbar.Superscript",
            "charmap" => "Umb.Tiptap.Toolbar.CharacterMap",
            "rtl" => "Umb.Tiptap.Toolbar.TextDirectionRtl",
            "ltr" => "Umb.Tiptap.Toolbar.TextDirectionLtr",
            "link" => "Umb.Tiptap.Toolbar.Link",
            "unlink" => "Umb.Tiptap.Toolbar.Unlink",
            "sourcecode" => "Umb.Tiptap.Toolbar.SourceEditor",
            "umbmediapicker" => "Umb.Tiptap.Toolbar.MediaPicker",
            "umbembeddialog" => "Umb.Tiptap.Toolbar.EmbeddedMedia",
            "umbblockpicker" => "Umb.Tiptap.Toolbar.BlockPicker",
            _ => item
        };
    }
}
