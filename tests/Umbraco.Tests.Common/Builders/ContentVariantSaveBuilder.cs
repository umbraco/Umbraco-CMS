// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public class ContentVariantSaveBuilder<TParent> : ChildBuilderBase<TParent, ContentVariantSave>,
    IWithNameBuilder,
    IWithCultureInfoBuilder
{
    private readonly List<ContentPropertyBasicBuilder<ContentVariantSaveBuilder<TParent>>> _propertyBuilders = new();
    private CultureInfo _cultureInfo;

    private string _name;
    private bool? _publish;
    private bool? _save;

    public ContentVariantSaveBuilder(TParent parentBuilder)
        : base(parentBuilder)
    {
    }

    CultureInfo IWithCultureInfoBuilder.CultureInfo
    {
        get => _cultureInfo;
        set => _cultureInfo = value;
    }

    string IWithNameBuilder.Name
    {
        get => _name;
        set => _name = value;
    }

    public ContentVariantSaveBuilder<TParent> WithSave(bool save)
    {
        _save = save;
        return this;
    }

    public ContentVariantSaveBuilder<TParent> WithPublish(bool publish)
    {
        _publish = publish;
        return this;
    }

    public ContentPropertyBasicBuilder<ContentVariantSaveBuilder<TParent>> AddProperty()
    {
        var builder = new ContentPropertyBasicBuilder<ContentVariantSaveBuilder<TParent>>(this);
        _propertyBuilders.Add(builder);
        return builder;
    }

    public override ContentVariantSave Build()
    {
        var name = _name;
        var culture = _cultureInfo?.Name;
        var save = _save ?? true;
        var publish = _publish ?? true;
        var properties = _propertyBuilders.Select(x => x.Build());

        return new ContentVariantSave
        {
            Name = name,
            Culture = culture,
            Save = save,
            Publish = publish,
            Properties = properties
        };
    }
}
