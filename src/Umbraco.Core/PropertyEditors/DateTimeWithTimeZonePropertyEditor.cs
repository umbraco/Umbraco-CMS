// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

[DataEditor(
    Constants.PropertyEditors.Aliases.DateTimeWithTimeZone,
    ValueEditorIsReusable = true)]
public class DateTimeWithTimeZonePropertyEditor : DataEditor
{
    private readonly IIOHelper _ioHelper;

    public DateTimeWithTimeZonePropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper ioHelper)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        SupportsReadOnly = true;
    }

    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new DateWithTimeZoneConfigurationEditor(_ioHelper);
}
