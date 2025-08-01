// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

internal class DateWithTimeZoneConfigurationEditor : ConfigurationEditor<DateWithTimeZoneConfiguration>
{
    public DateWithTimeZoneConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }
}
