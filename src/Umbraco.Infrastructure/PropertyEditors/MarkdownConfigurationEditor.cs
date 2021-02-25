// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors
{
    /// <summary>
    /// Represents the configuration editorfor the markdown value editor.
    /// </summary>
    internal class MarkdownConfigurationEditor : ConfigurationEditor<MarkdownConfiguration>
    {
        public MarkdownConfigurationEditor(IIOHelper ioHelper) : base(ioHelper)
        {
        }
    }
}
