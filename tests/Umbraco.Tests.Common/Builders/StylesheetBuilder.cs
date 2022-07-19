// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Tests.Common.Builders;

public class StylesheetBuilder
    : BuilderBase<Stylesheet>
{
    private string _content;
    private string _path;

    public StylesheetBuilder WithPath(string path)
    {
        _path = path;
        return this;
    }

    public StylesheetBuilder WithContent(string content)
    {
        _content = content;
        return this;
    }

    public override Stylesheet Build()
    {
        var path = _path ?? string.Empty;
        var content = _content ?? string.Empty;

        return new Stylesheet(path) { Content = content };
    }
}
