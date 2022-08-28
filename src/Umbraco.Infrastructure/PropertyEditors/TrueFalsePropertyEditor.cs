// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a checkbox property and parameter editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.Boolean,
    EditorType.PropertyValue | EditorType.MacroParameter,
    "Toggle",
    "boolean",
    ValueType = ValueTypes.Integer,
    Group = Constants.PropertyEditors.Groups.Common,
    Icon = "icon-checkbox",
    ValueEditorIsReusable = true)]
public class TrueFalsePropertyEditor : DataEditor
{
    private readonly IEditorConfigurationParser _editorConfigurationParser;
    private readonly IIOHelper _ioHelper;

    // Scheduled for removal in v12
    [Obsolete("Please use constructor that takes an IEditorConfigurationParser instead")]
    public TrueFalsePropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper ioHelper)
        : this(dataValueEditorFactory, ioHelper, StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TrueFalsePropertyEditor" /> class.
    /// </summary>
    public TrueFalsePropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper ioHelper,
        IEditorConfigurationParser editorConfigurationParser)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        _editorConfigurationParser = editorConfigurationParser;
        SupportsReadOnly = true;
    }

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new TrueFalseConfigurationEditor(_ioHelper, _editorConfigurationParser);
}
