// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Core.PropertyEditors;

// Scheduled for removal in v12
[Obsolete("Please use BlockListPropertyEditorBase instead")]

public abstract class BlockEditorPropertyEditor : BlockListPropertyEditorBase
{
    public const string ContentTypeKeyPropertyKey = "contentTypeKey";
    public const string UdiPropertyKey = "udi";

    [Obsolete("Use non-obsoleted ctor. This will be removed in Umbraco 13.")]
    protected BlockEditorPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        PropertyEditorCollection propertyEditors)
        : this(
            dataValueEditorFactory,
            propertyEditors,
            StaticServiceProvider.Instance.GetRequiredService<IBlockValuePropertyIndexValueFactory>())
    {

    }

    protected BlockEditorPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        PropertyEditorCollection propertyEditors,
        IBlockValuePropertyIndexValueFactory blockValuePropertyIndexValueFactory)
        : base(dataValueEditorFactory, blockValuePropertyIndexValueFactory)
    {
        PropertyEditors = propertyEditors;
    }

    private PropertyEditorCollection PropertyEditors { get; }
}
