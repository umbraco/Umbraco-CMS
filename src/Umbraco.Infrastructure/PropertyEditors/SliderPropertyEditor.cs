// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a slider editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.Slider,
    "Slider",
    "slider",
    Icon = "icon-navigation-horizontal",
    ValueEditorIsReusable = true)]
public class SliderPropertyEditor : DataEditor
{
    private readonly IEditorConfigurationParser _editorConfigurationParser;
    private readonly IIOHelper _ioHelper;

    // Scheduled for removal in v12
    [Obsolete("Please use constructor that takes an IEditorConfigurationParser instead")]
    public SliderPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper ioHelper)
        : this(dataValueEditorFactory, ioHelper, StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SliderPropertyEditor" /> class.
    /// </summary>
    public SliderPropertyEditor(
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
        new SliderConfigurationEditor(_ioHelper, _editorConfigurationParser);
}
