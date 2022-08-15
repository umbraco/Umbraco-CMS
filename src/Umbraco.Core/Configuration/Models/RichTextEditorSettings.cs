using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Core.Configuration.Models;

[UmbracoOptions(Constants.Configuration.ConfigRichTextEditor)]
public class RichTextEditorSettings
{
    internal const string StaticValidElements =
        "+a[id|style|rel|data-id|data-udi|rev|charset|hreflang|dir|lang|tabindex|accesskey|type|name|href|target|title|class|onfocus|onblur|onclick|ondblclick|onmousedown|onmouseup|onmouseover|onmousemove|onmouseout|onkeypress|onkeydown|onkeyup],-strong/-b[class|style],-em/-i[class|style],-strike[class|style],-u[class|style],#p[id|style|dir|class|align],-ol[class|reversed|start|style|type],-ul[class|style],-li[class|style],br[class],img[id|dir|lang|longdesc|usemap|style|class|src|onmouseover|onmouseout|border|alt=|title|hspace|vspace|width|height|align|umbracoorgwidth|umbracoorgheight|onresize|onresizestart|onresizeend|rel|data-id],-sub[style|class],-sup[style|class],-blockquote[dir|style|class],-table[border=0|cellspacing|cellpadding|width|height|class|align|summary|style|dir|id|lang|bgcolor|background|bordercolor],-tr[id|lang|dir|class|rowspan|width|height|align|valign|style|bgcolor|background|bordercolor],tbody[id|class],thead[id|class],tfoot[id|class],#td[id|lang|dir|class|colspan|rowspan|width|height|align|valign|style|bgcolor|background|bordercolor|scope],-th[id|lang|dir|class|colspan|rowspan|width|height|align|valign|style|scope],caption[id|lang|dir|class|style],-div[id|dir|class|align|style],-span[class|align|style],-pre[class|align|style],address[class|align|style],-h1[id|dir|class|align|style],-h2[id|dir|class|align|style],-h3[id|dir|class|align|style],-h4[id|dir|class|align|style],-h5[id|dir|class|align|style],-h6[id|style|dir|class|align|style],hr[class|style],small[class|style],dd[id|class|title|style|dir|lang],dl[id|class|title|style|dir|lang],dt[id|class|title|style|dir|lang],object[class|id|width|height|codebase|*],param[name|value|_value|class],embed[type|width|height|src|class|*],map[name|class],area[shape|coords|href|alt|target|class],bdo[class],button[class],iframe[*],figure,figcaption";

    internal const string StaticInvalidElements = "font";

    private static readonly string[] Default_plugins =
    {
        "paste", "anchor", "charmap", "table", "lists", "advlist", "hr", "autolink", "directionality", "tabfocus",
        "searchreplace",
    };

    private static readonly RichTextEditorCommand[] Default_commands =
    {
        new RichTextEditorCommand
        {
            Alias = "ace", Name = "Source code editor", Mode = RichTextEditorCommandMode.Insert,
        },
        new RichTextEditorCommand
        {
            Alias = "removeformat", Name = "Remove format", Mode = RichTextEditorCommandMode.Selection,
        },
        new RichTextEditorCommand { Alias = "undo", Name = "Undo", Mode = RichTextEditorCommandMode.Insert },
        new RichTextEditorCommand { Alias = "redo", Name = "Redo", Mode = RichTextEditorCommandMode.Insert },
        new RichTextEditorCommand { Alias = "cut", Name = "Cut", Mode = RichTextEditorCommandMode.Selection },
        new RichTextEditorCommand { Alias = "copy", Name = "Copy", Mode = RichTextEditorCommandMode.Selection },
        new RichTextEditorCommand { Alias = "paste", Name = "Paste", Mode = RichTextEditorCommandMode.All },
        new RichTextEditorCommand
        {
            Alias = "styleselect", Name = "Style select", Mode = RichTextEditorCommandMode.All,
        },
        new RichTextEditorCommand { Alias = "bold", Name = "Bold", Mode = RichTextEditorCommandMode.Selection },
        new RichTextEditorCommand { Alias = "italic", Name = "Italic", Mode = RichTextEditorCommandMode.Selection },
        new RichTextEditorCommand
        {
            Alias = "underline", Name = "Underline", Mode = RichTextEditorCommandMode.Selection,
        },
        new RichTextEditorCommand
        {
            Alias = "strikethrough", Name = "Strikethrough", Mode = RichTextEditorCommandMode.Selection,
        },
        new RichTextEditorCommand
        {
            Alias = "alignleft", Name = "Justify left", Mode = RichTextEditorCommandMode.Selection,
        },
        new RichTextEditorCommand
        {
            Alias = "aligncenter", Name = "Justify center", Mode = RichTextEditorCommandMode.Selection,
        },
        new RichTextEditorCommand
        {
            Alias = "alignright", Name = "Justify right", Mode = RichTextEditorCommandMode.Selection,
        },
        new RichTextEditorCommand
        {
            Alias = "alignjustify", Name = "Justify full", Mode = RichTextEditorCommandMode.Selection,
        },
        new RichTextEditorCommand { Alias = "bullist", Name = "Bullet list", Mode = RichTextEditorCommandMode.All },
        new RichTextEditorCommand { Alias = "numlist", Name = "Numbered list", Mode = RichTextEditorCommandMode.All },
        new RichTextEditorCommand
        {
            Alias = "outdent", Name = "Decrease indent", Mode = RichTextEditorCommandMode.All,
        },
        new RichTextEditorCommand
        {
            Alias = "indent", Name = "Increase indent", Mode = RichTextEditorCommandMode.All,
        },
        new RichTextEditorCommand { Alias = "link", Name = "Insert/edit link", Mode = RichTextEditorCommandMode.All },
        new RichTextEditorCommand
        {
            Alias = "unlink", Name = "Remove link", Mode = RichTextEditorCommandMode.Selection,
        },
        new RichTextEditorCommand { Alias = "anchor", Name = "Anchor", Mode = RichTextEditorCommandMode.Selection },
        new RichTextEditorCommand
        {
            Alias = "umbmediapicker", Name = "Image", Mode = RichTextEditorCommandMode.Insert,
        },
        new RichTextEditorCommand { Alias = "umbmacro", Name = "Macro", Mode = RichTextEditorCommandMode.All },
        new RichTextEditorCommand { Alias = "table", Name = "Table", Mode = RichTextEditorCommandMode.Insert },
        new RichTextEditorCommand
        {
            Alias = "umbembeddialog", Name = "Embed", Mode = RichTextEditorCommandMode.Insert,
        },
        new RichTextEditorCommand { Alias = "hr", Name = "Horizontal rule", Mode = RichTextEditorCommandMode.Insert },
        new RichTextEditorCommand
        {
            Alias = "subscript", Name = "Subscript", Mode = RichTextEditorCommandMode.Selection,
        },
        new RichTextEditorCommand
        {
            Alias = "superscript", Name = "Superscript", Mode = RichTextEditorCommandMode.Selection,
        },
        new RichTextEditorCommand
        {
            Alias = "charmap", Name = "Character map", Mode = RichTextEditorCommandMode.Insert,
        },
        new RichTextEditorCommand
        {
            Alias = "rtl", Name = "Right to left", Mode = RichTextEditorCommandMode.Selection,
        },
        new RichTextEditorCommand
        {
            Alias = "ltr", Name = "Left to right", Mode = RichTextEditorCommandMode.Selection,
        },
    };

    private static readonly IDictionary<string, string> Default_custom_config =
        new Dictionary<string, string> { ["entity_encoding"] = "raw" };

    /// <summary>
    ///     HTML RichText Editor TinyMCE Commands
    /// </summary>
    /// WB-TODO Custom Array of objects
    public RichTextEditorCommand[] Commands { get; set; } = Default_commands;

    /// <summary>
    ///     HTML RichText Editor TinyMCE Plugins
    /// </summary>
    public string[] Plugins { get; set; } = Default_plugins;

    /// <summary>
    ///     HTML RichText Editor TinyMCE Custom Config
    /// </summary>
    /// WB-TODO Custom Dictionary
    public IDictionary<string, string> CustomConfig { get; set; } = Default_custom_config;

    /// <summary>
    /// </summary>
    [DefaultValue(StaticValidElements)]
    public string ValidElements { get; set; } = StaticValidElements;

    /// <summary>
    ///     Invalid HTML elements for RichText Editor
    /// </summary>
    [DefaultValue(StaticInvalidElements)]
    public string InvalidElements { get; set; } = StaticInvalidElements;

    public class RichTextEditorCommand
    {
        [Required]
        public string Alias { get; set; } = null!;

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public RichTextEditorCommandMode Mode { get; set; }
    }
}
