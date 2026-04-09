using System.ComponentModel.DataAnnotations;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.Validation;

/// <summary>
/// Provides utility methods for extracting JSON path validation errors from validation results.
/// </summary>
internal static class JsonPathValidator
{
    /// <summary>
    /// Extracts JSON path validation errors from the specified validation result.
    /// </summary>
    /// <param name="validationResult">The validation result to extract errors from.</param>
    /// <returns>A collection of JSON path validation errors.</returns>
    public static IEnumerable<JsonPathValidationError> ExtractJsonPathValidationErrors(ValidationResult validationResult)
    {
        var root = new JsonPathValidationTreeItem { JsonPath = string.Empty };
        BuildJsonPathValidationTreeRecursively(validationResult, root);
        return ExtractJsonPathValidationErrorsRecursively(root);
    }

    private static void BuildJsonPathValidationTreeRecursively(ValidationResult validationResult, JsonPathValidationTreeItem current)
    {
        if (validationResult is not INestedValidationResults nestedValidationResults || nestedValidationResults.ValidationResults.Any() is false)
        {
            return;
        }

        if (validationResult is IJsonPathValidationResult jsonPathValidationResult)
        {
            current.Children.Add(new JsonPathValidationTreeItem
            {
                JsonPath = $"{current.JsonPath}.{jsonPathValidationResult.JsonPath}",
            });
            current = current.Children.Last();
        }

        current.ErrorMessages.AddRange(
            nestedValidationResults.ValidationResults
                .Select(child => child.ErrorMessage?.NullOrWhiteSpaceAsNull())
                .WhereNotNull());

        foreach (ValidationResult child in nestedValidationResults.ValidationResults)
        {
            BuildJsonPathValidationTreeRecursively(child, current);
        }
    }

    private static IEnumerable<JsonPathValidationError> ExtractJsonPathValidationErrorsRecursively(JsonPathValidationTreeItem current)
    {
        var errors = new List<JsonPathValidationError>();
        if (current.ErrorMessages.Any())
        {
            errors.Add(new JsonPathValidationError { ErrorMessages = current.ErrorMessages, JsonPath = current.JsonPath });
        }

        foreach (JsonPathValidationTreeItem child in current.Children)
        {
            errors.AddRange(ExtractJsonPathValidationErrorsRecursively(child));
        }

        return errors;
    }

    private sealed class JsonPathValidationTreeItem
    {
        public required string JsonPath { get; init; }

        public List<string> ErrorMessages { get; } = [];

        public List<JsonPathValidationTreeItem> Children { get; } = [];
    }
}
