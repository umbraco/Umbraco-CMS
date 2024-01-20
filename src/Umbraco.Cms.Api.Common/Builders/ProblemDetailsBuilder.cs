using Microsoft.AspNetCore.Mvc;

namespace Umbraco.Cms.Api.Common.Builders;

public class ProblemDetailsBuilder
{
    private string? _title;
    private string? _detail;
    private string? _type;
    private string? _operationStatus;
    private IDictionary<string, object>? _extensions;

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
        => WithExtension("propertyValidationErrors", propertyValidationErrors);

    public ProblemDetailsBuilder WithExtension(string key, object value)
    {
        _extensions ??= new Dictionary<string, object>();
        _extensions[key] = value;
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
            problemDetails.Extensions["operationStatus"] = _operationStatus;
        }

        if (_extensions is not null)
        {
            foreach (KeyValuePair<string, object> extension in _extensions)
            {
                problemDetails.Extensions[extension.Key] = extension.Value;
            }
        }

        return problemDetails;
    }
}
