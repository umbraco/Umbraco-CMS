// Copyright (c) Umbraco.
// See LICENSE for more details.
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a property editor for no configuration string properties.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.String,
    "String",
    "value",
    Icon = "umb:edit",
    ValueEditorIsReusable = true)]
public class StringPropertyEditor : DataEditor
{
    private readonly IEditorConfigurationParser _editorConfigurationParser;
    private readonly IIOHelper _ioHelper;

    /// <summary>
    ///     Initializes a new instance of the <see cref="StringPropertyEditor" /> class.
    /// </summary>
    public StringPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper ioHelper,
        IEditorConfigurationParser editorConfigurationParser)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        _editorConfigurationParser = editorConfigurationParser;
        SupportsReadOnly = true;
    }
}
