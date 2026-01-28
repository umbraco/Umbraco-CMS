// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public class TemplateBuilder
    : ChildBuilderBase<ContentTypeBuilder, ITemplate>,
        IWithIdBuilder,
        IWithKeyBuilder,
        IWithAliasBuilder,
        IWithNameBuilder,
        IWithCreateDateBuilder,
        IWithUpdateDateBuilder,
        IWithPathBuilder
{
    private string _alias;
    private string _content;
    private DateTime? _createDate;
    private int? _id;
    private bool? _isLayout;
    private Guid? _key;
    private string _layoutAlias;
    private Lazy<int> _layoutId;
    private string _name;
    private string _path;
    private DateTime? _updateDate;

    public TemplateBuilder()
        : base(null)
    {
    }

    public TemplateBuilder(ContentTypeBuilder parentBuilder)
        : base(parentBuilder)
    {
    }

    string IWithAliasBuilder.Alias
    {
        get => _alias;
        set => _alias = value;
    }

    DateTime? IWithCreateDateBuilder.CreateDate
    {
        get => _createDate;
        set => _createDate = value;
    }

    int? IWithIdBuilder.Id
    {
        get => _id;
        set => _id = value;
    }

    Guid? IWithKeyBuilder.Key
    {
        get => _key;
        set => _key = value;
    }

    string IWithNameBuilder.Name
    {
        get => _name;
        set => _name = value;
    }

    string IWithPathBuilder.Path
    {
        get => _path;
        set => _path = value;
    }

    DateTime? IWithUpdateDateBuilder.UpdateDate
    {
        get => _updateDate;
        set => _updateDate = value;
    }

    public TemplateBuilder WithContent(string content)
    {
        _content = content;
        return this;
    }

    public TemplateBuilder AsLayout(string layoutAlias, int layoutId)
    {
        _isLayout = true;
        _layoutAlias = layoutAlias;
        _layoutId = new Lazy<int>(() => layoutId);
        return this;
    }

    [Obsolete("Use AsLayout instead. This will be removed in Umbraco 19.")]
    public TemplateBuilder AsMasterTemplate(string masterTemplateAlias, int masterTemplateId)
        => AsLayout(masterTemplateAlias, masterTemplateId);

    public override ITemplate Build()
    {
        var id = _id ?? 0;
        var key = _key ?? Guid.NewGuid();
        var name = _name ?? Guid.NewGuid().ToString();
        var alias = _alias ?? name.ToCamelCase();
        var createDate = _createDate ?? DateTime.UtcNow;
        var updateDate = _updateDate ?? DateTime.UtcNow;
        var path = _path ?? $"-1,{id}";
        var content = _content;
        var isLayout = _isLayout ?? false;
        var layoutAlias = _layoutAlias ?? string.Empty;
        var layoutId = _layoutId ?? new Lazy<int>(() => -1);

        var shortStringHelper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig());

        var template = new Template(shortStringHelper, name, alias)
        {
            Id = id,
            Key = key,
            CreateDate = createDate,
            UpdateDate = updateDate,
            Path = path,
            Content = content,
            IsLayout = isLayout,
            LayoutAlias = layoutAlias,
            LayoutId = layoutId
        };

        return template;
    }

    public static Template CreateTextPageTemplate(string alias = "textPage", string name = "Text page") =>
        (Template)new TemplateBuilder()
            .WithAlias(alias)
            .WithName(name)
            .Build();
}
