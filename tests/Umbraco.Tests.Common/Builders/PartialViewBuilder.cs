// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Tests.Common.Builders;

public class PartialViewBuilder
    : BuilderBase<IPartialView>
{
    private string _path;
    private string _content;

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

    public override IPartialView Build()
    {
        var path = _path ?? string.Empty;
        var content = _content ?? string.Empty;

        return new PartialView(path) { Content = content };
    }
}
