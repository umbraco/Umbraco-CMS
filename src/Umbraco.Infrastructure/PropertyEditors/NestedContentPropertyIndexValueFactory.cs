// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.PropertyEditors;

internal sealed class NestedContentPropertyIndexValueFactory
    : NestedPropertyIndexValueFactoryBase<
            NestedContentPropertyEditor.NestedContentValues.NestedContentRowValue[],
            NestedContentPropertyEditor.NestedContentValues.NestedContentRowValue>,
        INestedContentPropertyIndexValueFactory
{
    private readonly IContentTypeService _contentTypeService;


    public NestedContentPropertyIndexValueFactory(
        PropertyEditorCollection propertyEditorCollection,
        IContentTypeService contentTypeService,
        IJsonSerializer jsonSerializer) : base(propertyEditorCollection, jsonSerializer)
    {
        _contentTypeService = contentTypeService;
    }

    protected override IContentType? GetContentTypeOfNestedItem(
        NestedContentPropertyEditor.NestedContentValues.NestedContentRowValue input)
        => _contentTypeService.Get(input.ContentTypeAlias);

    protected override IDictionary<string, object?> GetRawProperty(
        NestedContentPropertyEditor.NestedContentValues.NestedContentRowValue nestedContentRowValue) =>
        nestedContentRowValue.RawPropertyValues;

    protected override IEnumerable<NestedContentPropertyEditor.NestedContentValues.NestedContentRowValue> GetDataItems(
        NestedContentPropertyEditor.NestedContentValues.NestedContentRowValue[] input) => input;
}
