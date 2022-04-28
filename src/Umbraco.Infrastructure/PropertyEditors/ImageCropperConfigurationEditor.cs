// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Core.PropertyEditors
{
    /// <summary>
    /// Represents the configuration editor for the image cropper value editor.
    /// </summary>
    internal class ImageCropperConfigurationEditor : ConfigurationEditor<ImageCropperConfiguration>
    {
        /// <inheritdoc />
        public override IDictionary<string, object> ToValueEditor(object? configuration)
        {
            var d = base.ToValueEditor(configuration);
            if (!d.ContainsKey("focalPoint")) d["focalPoint"] = new { left = 0.5, top = 0.5 };
            if (!d.ContainsKey("src")) d["src"] = "";
            return d;
        }

        // Scheduled for removal in v12
        [Obsolete("Please use constructor that takes an IEditorConfigurationParser instead")]
        public ImageCropperConfigurationEditor(IIOHelper ioHelper)
            : this(ioHelper, StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
        {
        }

        public ImageCropperConfigurationEditor(IIOHelper ioHelper, IEditorConfigurationParser editorConfigurationParser) : base(ioHelper, editorConfigurationParser)
        {
        }
    }
}
