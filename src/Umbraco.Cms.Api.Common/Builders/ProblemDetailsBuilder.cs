using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Umbraco.Cms.Api.Common.Builders;

public class ProblemDetailsBuilder
{
    private string? _title;
    private string? _detail;
    private string? _type;
    private ModelStateDictionary? _modelState;

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

    public ProblemDetailsBuilder WithType(ModelStateDictionary modelState)
    {
        _modelState = modelState;
        return this;
    }

    public ProblemDetails Build()
    {
        var model = _modelState is null ? new ProblemDetails() : new ValidationProblemDetails();

        model.Title = _title;
        model.Detail = _detail;
        model.Type = _type ?? "Error";

        return model;
    }
}
