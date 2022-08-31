// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Core.PropertyEditors;

[DataEditor(
    Constants.PropertyEditors.Aliases.DropDownListFlexible,
    "Dropdown",
    "dropdownFlexible",
    Group = Constants.PropertyEditors.Groups.Lists,
    Icon = "icon-indent",
    ValueEditorIsReusable = true)]
public class DropDownFlexiblePropertyEditor : DataEditor
{
    private readonly IEditorConfigurationParser _editorConfigurationParser;
    private readonly IIOHelper _ioHelper;
    private readonly ILocalizedTextService _textService;

    // Scheduled for removal in v12
    [Obsolete("Please use constructor that takes an IEditorConfigurationParser instead")]
    public DropDownFlexiblePropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        ILocalizedTextService textService,
        IIOHelper ioHelper)
        : this(
            dataValueEditorFactory,
            textService,
            ioHelper,
            StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
    {
    }

    public DropDownFlexiblePropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        ILocalizedTextService textService,
        IIOHelper ioHelper,
        IEditorConfigurationParser editorConfigurationParser)
        : base(dataValueEditorFactory)
    {
        _textService = textService;
        _ioHelper = ioHelper;
        _editorConfigurationParser = editorConfigurationParser;
        SupportsReadOnly = true;
    }

    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<MultipleValueEditor>(Attribute!);

    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new DropDownFlexibleConfigurationEditor(_textService, _ioHelper, _editorConfigurationParser);
}
