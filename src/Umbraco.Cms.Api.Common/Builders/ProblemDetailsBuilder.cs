using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Common.Builders;

/// <summary>
///     A fluent builder for creating RFC 7807 <see cref="ProblemDetails"/> responses.
/// </summary>
public class ProblemDetailsBuilder
{
    private string? _title;
    private string? _detail;
    private string? _type;
    private string? _operationStatus;
    private IDictionary<string, object>? _extensions;

    /// <summary>
    ///     Sets the title of the problem details.
    /// </summary>
    /// <param name="title">A short, human-readable summary of the problem type.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public ProblemDetailsBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    /// <summary>
    ///     Sets the detail of the problem details.
    /// </summary>
    /// <param name="detail">A human-readable explanation specific to this occurrence of the problem.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public ProblemDetailsBuilder WithDetail(string detail)
    {
        _detail = detail;
        return this;
    }

    /// <summary>
    ///     Sets the type of the problem details.
    /// </summary>
    /// <param name="type">A URI reference that identifies the problem type.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public ProblemDetailsBuilder WithType(string type)
    {
        _type = type;
        return this;
    }

    /// <summary>
    ///     Sets the operation status from an enum value.
    /// </summary>
    /// <typeparam name="TEnum">The enum type representing operation statuses.</typeparam>
    /// <param name="operationStatus">The operation status enum value.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public ProblemDetailsBuilder WithOperationStatus<TEnum>(TEnum operationStatus)
        where TEnum : Enum
    {
        _operationStatus = operationStatus.ToString();
        return this;
    }

    /// <summary>
    ///     Adds request model validation errors to the problem details.
    /// </summary>
    /// <param name="errors">A dictionary of field names to error messages.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public ProblemDetailsBuilder WithRequestModelErrors(IDictionary<string, string[]> errors)
        => WithExtension(nameof(HttpValidationProblemDetails.Errors).ToFirstLowerInvariant(), errors);

    /// <summary>
    ///     Adds a custom extension to the problem details.
    /// </summary>
    /// <param name="key">The extension key.</param>
    /// <param name="value">The extension value.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public ProblemDetailsBuilder WithExtension(string key, object value)
    {
        _extensions ??= new Dictionary<string, object>();
        _extensions[key] = value;
        return this;
    }

    /// <summary>
    ///     Builds the <see cref="ProblemDetails"/> instance with all configured values.
    /// </summary>
    /// <returns>A new <see cref="ProblemDetails"/> instance.</returns>
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
