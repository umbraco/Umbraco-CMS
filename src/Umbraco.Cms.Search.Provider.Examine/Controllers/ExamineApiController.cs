using Asp.Versioning;
using Examine;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Search.Core.Extensions;
using Umbraco.Cms.Search.Provider.Examine.Helpers;
using Umbraco.Cms.Search.Provider.Examine.Models.ViewModels;
using Umbraco.Cms.Search.Provider.Examine.Services;

namespace Umbraco.Cms.Search.Provider.Examine.Controllers;

[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Umbraco Search (Examine Provider)")]
public class ExamineApiController : ExamineApiControllerBase
{
    private readonly IExamineManager _examineManager;
    private readonly IActiveIndexManager _activeIndexManager;

    public ExamineApiController(IExamineManager examineManager, IActiveIndexManager activeIndexManager)
    {
        _examineManager = examineManager;
        _activeIndexManager = activeIndexManager;
    }

    [HttpGet("{indexAlias}/document/{documentKey:guid}")]
    [ProducesResponseType<DocumentViewModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetDocument(string indexAlias, Guid documentKey)
    {
        var activeIndex = _activeIndexManager.ResolveActiveIndexName(indexAlias);
        if (_examineManager.TryGetIndex(activeIndex, out IIndex? index) is false)
        {
            return NotFound($"Could not find index with alias '{activeIndex}'");
        }

        ISearchResults results = index.Searcher
            .CreateQuery()
            .Field(
                FieldNameHelper.FieldName(Core.Constants.FieldNames.Id, Constants.FieldValues.Keywords),
                documentKey.AsKeyword())
            .Execute();

        if (results.Any() is false)
        {
            return NotFound(
                $"Could not find document with key '{documentKey}' in index '{activeIndex}'");
        }

        var indexDocumentViewModels = results.Select(result => new IndexDocumentViewModel()
            {
                Fields = result.AllValues
                    .Where(kvp => ShouldDisplayField(kvp.Key))
                    .Select(kvp => ParseField(kvp.Key, kvp.Value))
                    .ToList()
                    .AsReadOnly(),
            })
            .ToList();

        var documentViewModel = new DocumentViewModel()
        {
            Key = documentKey,
            Documents = indexDocumentViewModels,
        };


        return Ok(documentViewModel);

        // determines if a field should be displayed to the users:
        // - system fields (including our own) should be omitted. they all start with "__".
        bool ShouldDisplayField(string fieldName) => fieldName.StartsWith("__") is false;
    }

    private static FieldViewModel ParseField(string fieldName, IEnumerable<string> values)
    {
        // Strip "Field_" prefix if present
        var cleanName = fieldName.StartsWith("Field_")
            ? fieldName[6..]
            : fieldName;

        // Extract field type suffix (e.g., "_keywords", "_texts")
        string? fieldType = null;
        var parts = cleanName.Split('_');
        if (parts.Length > 1)
        {
            var lastPart = parts[^1];
            if (IsFieldTypeSuffix(lastPart))
            {
                fieldType = lastPart;
                cleanName = string.Join("_", parts[..^1]);
            }
        }

        return new FieldViewModel { Name = cleanName, Type = fieldType, Values = values.ToList().AsReadOnly(), };
    }

    private static bool IsFieldTypeSuffix(string suffix) =>
        suffix is Constants.FieldValues.Keywords or Constants.FieldValues.Texts or Constants.FieldValues.TextsR1
            or Constants.FieldValues.TextsR2 or Constants.FieldValues.TextsR3 or Constants.FieldValues.Integers
            or Constants.FieldValues.Decimals or Constants.FieldValues.DateTimeOffsets;
}
