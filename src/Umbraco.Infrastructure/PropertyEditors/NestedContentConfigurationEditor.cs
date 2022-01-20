// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors
{
    /// <summary>
    /// Represents the configuration editor for the nested content value editor.
    /// </summary>
    public class NestedContentConfigurationEditor : ConfigurationEditor<NestedContentConfiguration>
    {
        public NestedContentConfigurationEditor(IIOHelper ioHelper) : base(ioHelper)
        {
        }
    }
}
