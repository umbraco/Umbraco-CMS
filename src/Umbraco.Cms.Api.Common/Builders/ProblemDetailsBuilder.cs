using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Umbraco.Cms.Api.Common.Builders;

public class ProblemDetailsBuilder
{
    private string? _title;
    private string? _detail;
    private string? _type;

    public ProblemDetailsBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public ProblemDetailsBuilder WithDetail(string detail)
    {
        _detail = detail;
        return this;
    }

    public ProblemDetailsBuilder WithType(string type)
    {
        _type = type;
        return this;
    }

    public ProblemDetails Build() =>
        new()
        {
            Title = _title,
            Detail = _detail,
            Type = _type ?? "Error",
        };
}
