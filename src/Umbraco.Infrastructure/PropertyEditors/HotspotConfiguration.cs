// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Runtime.Serialization;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the hotspot value editor.
/// </summary>
public class HotspotConfiguration
{
    [ConfigurationField("mediaId", "Image", "mediapicker", Description = "Choose the image to select hotspots.")]
    public GuidUdi? MediaId { get; set; }
}
