﻿// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors
{
    /// <summary>
    /// Represents the configuration editor for the media picker value editor.
    /// </summary>
    public class MediaPickerConfigurationEditor : ConfigurationEditor<MediaPickerConfiguration>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MediaPickerConfigurationEditor"/> class.
        /// </summary>
        public MediaPickerConfigurationEditor(IIOHelper ioHelper): base(ioHelper)
        {
            // configure fields
            // this is not part of ContentPickerConfiguration,
            // but is required to configure the UI editor (when editing the configuration)
            Field(nameof(MediaPickerConfiguration.StartNodeId))
                .Config = new Dictionary<string, object> { { "idType", "udi" } };
        }

        public override IDictionary<string, object> ToValueEditor(object configuration)
        {
            // get the configuration fields
            var d = base.ToValueEditor(configuration);

            // add extra fields
            // not part of ContentPickerConfiguration but used to configure the UI editor
            d["idType"] = "udi";

            return d;
        }
    }
}
