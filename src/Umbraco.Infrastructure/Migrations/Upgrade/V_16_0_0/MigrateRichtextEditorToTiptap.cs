using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_16_0_0;

public class MigrateRichtextEditorToTiptap : AsyncMigrationBase
{
    private readonly IDataTypeService _dataTypeService;

    public MigrateRichtextEditorToTiptap(IMigrationContext context, IDataTypeService dataTypeService) : base(context)
    {
        _dataTypeService = dataTypeService;
    }

    protected override async Task MigrateAsync()
    {
        IEnumerable<IDataType> dataTypes = await _dataTypeService.GetByEditorUiAlias("Umb.PropertyEditorUi.TinyMCE");

        foreach (IDataType dataType in dataTypes)
        {
            MigrateToTipTap(dataType.ConfigurationData);
            await _dataTypeService.UpdateAsync(dataType, Constants.Security.SuperUserKey);
        }

    }

    private void MigrateToTipTap(IDictionary<string, object> configurationObject)
    {

        if (!configurationObject.TryGetValue("toolbar", out var toolbar) || toolbar is not string toolbarString)
        {
            return;
        }

        var toolbarArray = toolbarString.Split(" ").Select(x => x.Trim()).ToArray();
        var newToolbar = toolbarArray.Select(MapToolbarItem).WhereNotNull().ToList();
        configurationObject["toolbar"] = new List<List<List<string>>> { new() {newToolbar} };
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
