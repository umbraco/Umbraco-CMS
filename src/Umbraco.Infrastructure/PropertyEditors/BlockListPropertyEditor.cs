// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a block list property editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.BlockList,
    ValueType = ValueTypes.Json,
    ValueEditorIsReusable = false)]
public class BlockListPropertyEditor : BlockEditorPropertyEditor
{
    private readonly IIOHelper _ioHelper;

    public BlockListPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        PropertyEditorCollection propertyEditors,
        IIOHelper ioHelper,
        IBlockValuePropertyIndexValueFactory blockValuePropertyIndexValueFactory,
        IJsonSerializer jsonSerializer)
        : base(dataValueEditorFactory, propertyEditors, blockValuePropertyIndexValueFactory, jsonSerializer)
        => _ioHelper = ioHelper;

    #region Pre Value Editor

    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new BlockListConfigurationEditor(_ioHelper);

    #endregion
}
