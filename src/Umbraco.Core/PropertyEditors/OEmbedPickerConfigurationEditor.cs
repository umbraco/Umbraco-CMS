// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.PropertyEditors
{
    /// <summary>
    /// Represents the configuration editor for the OEmbed picker editor.
    /// </summary>
    public class OEmbedPickerConfigurationEditor : ConfigurationEditor<OEmbedPickerConfiguration>
    {
        public OEmbedPickerConfigurationEditor(IIOHelper ioHelper, IEditorConfigurationParser editorConfigurationParser)
            : base(ioHelper, editorConfigurationParser)
        {

        }

        /// <summary>
        /// Gets the default configuration object.
        /// </summary>
        public override object DefaultConfigurationObject { get; } = new OEmbedPickerConfiguration { Multiple = false };
    }
}
