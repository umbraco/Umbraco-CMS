// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents a block list property editor.
/// </summary>
// TODO: Re-add this DataEditor attribute to re-enable the BlockGridEditor for V11/V10.4
// [DataEditor(
//     Constants.PropertyEditors.Aliases.BlockGrid,
//     "Block Grid",
//     "blockgrid",
//     ValueType = ValueTypes.Json,
//     Group = Constants.PropertyEditors.Groups.RichContent,
//     Icon = "icon-layout")]
[HideFromTypeFinder]
[EditorBrowsable(EditorBrowsableState.Never)]
public class BlockGridPropertyEditor : BlockGridPropertyEditorBase
{
    private readonly IIOHelper _ioHelper;

    public BlockGridPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper ioHelper)
        : base(dataValueEditorFactory) =>
        _ioHelper = ioHelper;

    #region Pre Value Editor

    protected override IConfigurationEditor CreateConfigurationEditor() => new BlockGridConfigurationEditor(_ioHelper);

    #endregion
}
