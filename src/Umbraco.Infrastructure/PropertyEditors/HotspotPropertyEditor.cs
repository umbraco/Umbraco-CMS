// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a hotspot property editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.Hotspot,
    "Hotspot",
    "hotspot",
    ValueType = ValueTypes.Json,
    HideLabel = false,
    Group = Constants.PropertyEditors.Groups.Media,
    Icon = "icon-crosshair",
    ValueEditorIsReusable = true)]
public class HotspotPropertyEditor : DataEditor
{
    private readonly UploadAutoFillProperties _autoFillProperties;
    private readonly IContentService _contentService;
    private readonly IDataTypeService _dataTypeService;
    private readonly IEditorConfigurationParser _editorConfigurationParser;
    private readonly IIOHelper _ioHelper;
    private readonly ILogger<HotspotPropertyEditor> _logger;
    private readonly MediaFileManager _mediaFileManager;
    private ContentSettings _contentSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="HotspotPropertyEditor" /> class.
    /// </summary>
    public HotspotPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        ILoggerFactory loggerFactory,
        MediaFileManager mediaFileManager,
        IOptionsMonitor<ContentSettings> contentSettings,
        IDataTypeService dataTypeService,
        IIOHelper ioHelper,
        UploadAutoFillProperties uploadAutoFillProperties,
        IContentService contentService,
        IEditorConfigurationParser editorConfigurationParser)
        : base(dataValueEditorFactory)
    {
        _mediaFileManager = mediaFileManager ?? throw new ArgumentNullException(nameof(mediaFileManager));
        _contentSettings = contentSettings.CurrentValue ?? throw new ArgumentNullException(nameof(contentSettings));
        _dataTypeService = dataTypeService ?? throw new ArgumentNullException(nameof(dataTypeService));
        _ioHelper = ioHelper ?? throw new ArgumentNullException(nameof(ioHelper));
        _autoFillProperties =
            uploadAutoFillProperties ?? throw new ArgumentNullException(nameof(uploadAutoFillProperties));
        _contentService = contentService;
        _editorConfigurationParser = editorConfigurationParser;
        _logger = loggerFactory.CreateLogger<HotspotPropertyEditor>();

        contentSettings.OnChange(x => _contentSettings = x);
        SupportsReadOnly = true;
    }

    public override IPropertyIndexValueFactory PropertyIndexValueFactory { get; } = new NoopPropertyIndexValueFactory();

    /// <summary>
    ///     Creates the corresponding property value editor.
    /// </summary>
    /// <returns>The corresponding property value editor.</returns>
    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<HotspotPropertyValueEditor>(Attribute!);

    /// <summary>
    ///     Creates the corresponding preValue editor.
    /// </summary>
    /// <returns>The corresponding preValue editor.</returns>
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new HotspotConfigurationEditor(_ioHelper, _editorConfigurationParser);
}
