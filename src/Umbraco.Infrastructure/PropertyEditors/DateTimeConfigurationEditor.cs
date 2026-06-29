// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

internal class DateTimeConfigurationEditor : ConfigurationEditor<DateTimeConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DateTimeConfigurationEditor"/> class.
    /// </summary>
    /// <param name="ioHelper">An instance of <see cref="IIOHelper"/> used for IO operations within the configuration editor.</param>
    public DateTimeConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }
}
