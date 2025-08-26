// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

internal class DateTime2ConfigurationEditor : ConfigurationEditor<DateTime2Configuration>
{
    public DateTime2ConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }
}
