// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors.Validators;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a date and time property editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.DateTime,
    "Date/Time",
    "datepicker",
    ValueType = ValueTypes.DateTime,
    Icon = "icon-time",
    ValueEditorIsReusable = true)]
public class DateTimePropertyEditor : DataEditor
{
    private readonly IEditorConfigurationParser _editorConfigurationParser;
    private readonly IIOHelper _ioHelper;

    public DateTimePropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper)
        : this(dataValueEditorFactory, ioHelper, StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DateTimePropertyEditor" /> class.
    /// </summary>
    public DateTimePropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper, IEditorConfigurationParser editorConfigurationParser)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        _editorConfigurationParser = editorConfigurationParser;
        SupportsReadOnly = true;
    }

    /// <inheritdoc />
    protected override IDataValueEditor CreateValueEditor()
    {
        IDataValueEditor editor = base.CreateValueEditor();
        editor.Validators.Add(new DateTimeValidator());
        return editor;
    }

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new DateTimeConfigurationEditor(_ioHelper, _editorConfigurationParser);
}
