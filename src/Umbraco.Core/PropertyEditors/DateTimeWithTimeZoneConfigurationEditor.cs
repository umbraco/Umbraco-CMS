// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

internal class DateTimeWithTimeZoneConfigurationEditor : ConfigurationEditor<DateTimeWithTimeZoneConfiguration>
{
    public DateTimeWithTimeZoneConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }
}
