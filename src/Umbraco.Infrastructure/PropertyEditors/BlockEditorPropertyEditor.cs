// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.PropertyEditors;

// Scheduled for removal in v12
[Obsolete("Please use BlockListPropertyEditorBase instead")]

public abstract class BlockEditorPropertyEditor : BlockListPropertyEditorBase
{
    public const string ContentTypeKeyPropertyKey = "contentTypeKey";
    public const string UdiPropertyKey = "udi";

    protected BlockEditorPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        PropertyEditorCollection propertyEditors)
        : base(dataValueEditorFactory) =>
        PropertyEditors = propertyEditors;

    private PropertyEditorCollection PropertyEditors { get; }
}
