// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents a block list property editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.BlockGrid,
    ValueType = ValueTypes.Json)]
public class BlockGridPropertyEditor : BlockGridPropertyEditorBase
{
    private readonly IIOHelper _ioHelper;

    public BlockGridPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper ioHelper,
        IBlockValuePropertyIndexValueFactory blockValuePropertyIndexValueFactory)
        : base(dataValueEditorFactory, blockValuePropertyIndexValueFactory)
        => _ioHelper = ioHelper;


    #region Pre Value Editor

    protected override IConfigurationEditor CreateConfigurationEditor() => new BlockGridConfigurationEditor(_ioHelper);

    #endregion
}
