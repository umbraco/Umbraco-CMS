using Microsoft.AspNetCore.Mvc;

namespace Umbraco.Cms.Api.Common.Builders;

public class ProblemDetailsBuilder
{
    private string? _title;
    private string? _detail;
    private string? _type;
    private string? _operationStatus;
    private IDictionary<string, string[]>? _propertyValidationErrors;

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

    public ProblemDetailsBuilder WithOperationStatus<TEnum>(TEnum operationStatus)
        where TEnum : Enum
    {
        _operationStatus = operationStatus.ToString();
        return this;
    }

    public ProblemDetailsBuilder WithPropertyValidationErrors(IDictionary<string, string[]> propertyValidationErrors)
    {
        _propertyValidationErrors = propertyValidationErrors.Any() ? propertyValidationErrors : null;
        return this;
    }

    public ProblemDetails Build()
    {
        var problemDetails = new ProblemDetails
        {
            Title = _title,
            Detail = _detail,
            Type = _type ?? "Error",
        };

        if (_operationStatus is not null)
        {
            problemDetails.Extensions["operationstatus"] = _operationStatus;
        }

        if (_propertyValidationErrors is not null)
        {
            problemDetails.Extensions["errors"] = _propertyValidationErrors;
        }

        return problemDetails;
    }
}
