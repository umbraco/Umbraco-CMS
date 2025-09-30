// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

internal class DateTimeConfigurationEditor : ConfigurationEditor<DateTimeConfiguration>
{
    public DateTimeConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }
}
