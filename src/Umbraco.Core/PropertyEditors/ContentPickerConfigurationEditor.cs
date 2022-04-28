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
    internal class ContentPickerConfigurationEditor : ConfigurationEditor<ContentPickerConfiguration>
    {
        // Scheduled for removal in v12
        [Obsolete("Please use constructor that takes an IEditorConfigurationParser instead")]
        public ContentPickerConfigurationEditor(IIOHelper ioHelper)
            : this(ioHelper, StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
        {
        }

        public ContentPickerConfigurationEditor(IIOHelper ioHelper, IEditorConfigurationParser editorConfigurationParser) : base(ioHelper, editorConfigurationParser)
        {
            // configure fields
            // this is not part of ContentPickerConfiguration,
            // but is required to configure the UI editor (when editing the configuration)
            Field(nameof(ContentPickerConfiguration.StartNodeId))
                .Config = new Dictionary<string, object> { { "idType", "udi" } };
        }

        public override IDictionary<string, object> ToValueEditor(object? configuration)
        {
            // get the configuration fields
            var d = base.ToValueEditor(configuration);

            // add extra fields
            // not part of ContentPickerConfiguration but used to configure the UI editor
            d["showEditButton"] = false;
            d["showPathOnHover"] = false;
            d["idType"] = "udi";

            return d;
        }
    }
}
