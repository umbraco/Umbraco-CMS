// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a block list property editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.BlockList,
    "Block List",
    "blocklist",
    ValueType = ValueTypes.Json,
    Group = Constants.PropertyEditors.Groups.Lists,
    Icon = "icon-thumbnail-list",
    ValueEditorIsReusable = false)]
public class BlockListPropertyEditor : BlockEditorPropertyEditor
{
    private readonly IEditorConfigurationParser _editorConfigurationParser;
    private readonly IIOHelper _ioHelper;


    [Obsolete("Use non-obsoleted ctor. This will be removed in Umbraco 15.")]
    public BlockListPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        PropertyEditorCollection propertyEditors,
        IIOHelper ioHelper,
        IEditorConfigurationParser editorConfigurationParser,
        IBlockValuePropertyIndexValueFactory blockValuePropertyIndexValueFactory)
        : this(dataValueEditorFactory, propertyEditors, ioHelper, editorConfigurationParser, blockValuePropertyIndexValueFactory, StaticServiceProvider.Instance.GetRequiredService<IJsonSerializer>())
    {

    }

    public BlockListPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        PropertyEditorCollection propertyEditors,
        IIOHelper ioHelper,
        IEditorConfigurationParser editorConfigurationParser,
        IBlockValuePropertyIndexValueFactory blockValuePropertyIndexValueFactory,
        IJsonSerializer jsonSerializer)
        : base(dataValueEditorFactory, propertyEditors, blockValuePropertyIndexValueFactory, jsonSerializer)
    {
        _ioHelper = ioHelper;
        _editorConfigurationParser = editorConfigurationParser;
    }

    #region Pre Value Editor

    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new BlockListConfigurationEditor(_ioHelper, _editorConfigurationParser);

    #endregion
}
