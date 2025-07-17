// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

public class DateTimeWithTimeZoneConfigurationEditor : ConfigurationEditor<DateTimeWithTimeZoneConfiguration>
{
    public DateTimeWithTimeZoneConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }

    /// <inheritdoc />
    public override IDictionary<string, object> ToConfigurationEditor(IDictionary<string, object> configuration)
    {
        return configuration;
    }

    public override IDictionary<string, object> ToValueEditor(IDictionary<string, object> configuration) => base.ToValueEditor(configuration);
}
