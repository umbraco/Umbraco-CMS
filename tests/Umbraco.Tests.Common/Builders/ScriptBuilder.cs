// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Tests.Common.Builders;

public class ScriptBuilder
    : BuilderBase<Script>
{
    private string _path;
    private string _content;

    public ScriptBuilder WithPath(string path)
    {
        _path = path;
        return this;
    }

    public ScriptBuilder WithContent(string content)
    {
        _content = content;
        return this;
    }

    public override Script Build()
    {
        var path = _path ?? string.Empty;
        var content = _content ?? string.Empty;

        return new Script(path) { Content = content };
    }
}
