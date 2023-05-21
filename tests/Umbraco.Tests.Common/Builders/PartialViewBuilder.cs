// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Tests.Common.Builders;

public class PartialViewBuilder
    : BuilderBase<IPartialView>
{
    private string _path;
    private string _content;
    private PartialViewType _viewType = PartialViewType.Unknown;

    public PartialViewBuilder WithPath(string path)
    {
        _path = path;
        return this;
    }

    public PartialViewBuilder WithContent(string content)
    {
        _content = content;
        return this;
    }

    public PartialViewBuilder WithViewType(PartialViewType viewType)
    {
        _viewType = viewType;
        return this;
    }

    public override IPartialView Build()
    {
        var path = _path ?? string.Empty;
        var content = _content ?? string.Empty;
        var viewType = _viewType;

        return new PartialView(viewType, path) { Content = content };
    }
}
