using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

internal class CodeEditorConfigurationEditor : ConfigurationEditor<CodeEditorConfiguration>
{
    internal const string Mode = "mode";
    internal const string Theme = "theme";

    public CodeEditorConfigurationEditor(
        IIOHelper ioHelper,
        IEditorConfigurationParser editorConfigurationParser)
        : base(ioHelper, editorConfigurationParser)
    {
    }

    /// <inheritdoc />
    public override Dictionary<string, object> ToConfigurationEditor(CodeEditorConfiguration? configuration) =>
        new()
        {
            { "mode", configuration?.Mode ?? "razor" },
            { "theme", configuration?.Theme ?? "chrome" },
        };

    /// <inheritdoc />
    public override CodeEditorConfiguration FromConfigurationEditor(
        IDictionary<string, object?>? editorValues, CodeEditorConfiguration? configuration)
    {
        string? mode = null;
        string? theme = null;

        if (editorValues is not null && editorValues.TryGetValue("mode", out var modeVal))
        {
            mode = modeVal?.ToString();
        }

        if (editorValues is not null && editorValues.TryGetValue("theme", out var themeVal))
        {
            theme = themeVal?.ToString();
        }

        return new CodeEditorConfiguration { Mode = mode, Theme = theme };
    }
}
