﻿// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration editor for the rich text value editor.
/// </summary>
public class RichTextConfigurationEditor : ConfigurationEditor<RichTextConfiguration>
{
    // Scheduled for removal in v12
    [Obsolete("Please use constructor that takes an IEditorConfigurationParser instead")]
    public RichTextConfigurationEditor(IIOHelper ioHelper)
        : this(ioHelper, StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
    {
    }

    public RichTextConfigurationEditor(IIOHelper ioHelper, IEditorConfigurationParser editorConfigurationParser)
        : base(ioHelper, editorConfigurationParser)
    {
    }
}
