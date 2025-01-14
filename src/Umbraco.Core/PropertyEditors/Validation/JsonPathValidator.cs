using System.ComponentModel.DataAnnotations;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.Validation;

internal static class JsonPathValidator
{
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

    private class JsonPathValidationTreeItem
    {
        public required string JsonPath { get; init; }

        public List<string> ErrorMessages { get; } = [];

        public List<JsonPathValidationTreeItem> Children { get; } = [];
    }
}
