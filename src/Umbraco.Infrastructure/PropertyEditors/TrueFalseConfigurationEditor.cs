// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors
{
    /// <summary>
    /// Represents the configuration editor for the boolean value editor.
    /// </summary>
    public class TrueFalseConfigurationEditor : ConfigurationEditor<TrueFalseConfiguration>
    {
        public TrueFalseConfigurationEditor(IIOHelper ioHelper) : base(ioHelper)
        {
        }
    }
}
