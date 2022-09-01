using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Umbraco.Cms.ManagementApi.Builders;

public class ProblemDetailsBuilder
{
    private string? _title;
    private string? _detail;
    private int _status = StatusCodes.Status400BadRequest;
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

    public ProblemDetailsBuilder WithStatus(int status)
    {
        _status = status;
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
            Status = _status,
            Type = _type ?? "Error",
        };
}
