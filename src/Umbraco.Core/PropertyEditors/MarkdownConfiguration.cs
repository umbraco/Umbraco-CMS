﻿using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the markdown value editor.
    /// </summary>
    public class MarkdownConfiguration
    {
        [ConfigurationField("preview", "Preview", "boolean", Description = "Display a live preview")]
        public bool DisplayLivePreview { get; set; }

        [ConfigurationField("defaultValue", "Default value", "textarea", Description = "If value is blank, the editor will show this")]
        public string DefaultValue { get; set; }
    }
}
