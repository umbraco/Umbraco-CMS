// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;

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
        IJsonSerializer jsonSerializer,
        IOptionsMonitor<IndexingSettings> indexingSettings)
        : base(propertyEditorCollection, jsonSerializer, indexingSettings)
    {
        _contentTypeService = contentTypeService;
    }

    [Obsolete("Use non-obsolete constructor. This will be removed in Umbraco 14.")]
    public NestedContentPropertyIndexValueFactory(
        PropertyEditorCollection propertyEditorCollection,
        IContentTypeService contentTypeService,
        IJsonSerializer jsonSerializer)
        : this(propertyEditorCollection, contentTypeService, jsonSerializer, StaticServiceProvider.Instance.GetRequiredService<IOptionsMonitor<IndexingSettings>>())
    {
        _contentTypeService = contentTypeService;
    }

    protected override IContentType? GetContentTypeOfNestedItem(
        NestedContentPropertyEditor.NestedContentValues.NestedContentRowValue input, IDictionary<Guid, IContentType> contentTypeDictionary)
        => contentTypeDictionary.Values.FirstOrDefault(x=>x.Alias.Equals(input.ContentTypeAlias));

    [Obsolete("Use non-obsolete overload, scheduled for removal in v14")]
    protected override IContentType? GetContentTypeOfNestedItem(
        NestedContentPropertyEditor.NestedContentValues.NestedContentRowValue input)
        => _contentTypeService.Get(input.ContentTypeAlias);

    protected override IDictionary<string, object?> GetRawProperty(
        NestedContentPropertyEditor.NestedContentValues.NestedContentRowValue nestedContentRowValue) =>
        nestedContentRowValue.RawPropertyValues;

    protected override IEnumerable<NestedContentPropertyEditor.NestedContentValues.NestedContentRowValue> GetDataItems(
        NestedContentPropertyEditor.NestedContentValues.NestedContentRowValue[] input) => input;
}
