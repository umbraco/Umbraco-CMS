﻿// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Core.PropertyEditors
{
    /// <summary>
    /// Represents the configuration editorfor the markdown value editor.
    /// </summary>
    internal class MarkdownConfigurationEditor : ConfigurationEditor<MarkdownConfiguration>
    {
        // Scheduled for removal in v12
        [Obsolete("Please use constructor that takes an IEditorConfigurationParser instead")]
        public MarkdownConfigurationEditor(IIOHelper ioHelper)
            : this(ioHelper, StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
        {
        }

        public MarkdownConfigurationEditor(IIOHelper ioHelper, IEditorConfigurationParser editorConfigurationParser) : base(ioHelper, editorConfigurationParser)
        {
        }
    }
}
