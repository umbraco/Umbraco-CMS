using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Template.Query;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Models.TemplateQuery;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Template.Query;

[ApiVersion("1.0")]
public class ExecuteTemplateQueryController : TemplateQueryControllerBase
{
    private readonly IPublishedContentQuery _publishedContentQuery;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly IPublishedValueFallback _publishedValueFallback;
    private readonly IContentTypeService _contentTypeService;

    private static readonly string _indent = $"{Environment.NewLine}    ";

    public ExecuteTemplateQueryController(
        IPublishedContentQuery publishedContentQuery,
        IVariationContextAccessor variationContextAccessor,
        IPublishedValueFallback publishedValueFallback,
        IContentTypeService contentTypeService)
    {
        _publishedContentQuery = publishedContentQuery;
        _variationContextAccessor = variationContextAccessor;
        _publishedValueFallback = publishedValueFallback;
        _contentTypeService = contentTypeService;
    }

    [HttpPost("execute")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(TemplateQueryResultResponseModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<TemplateQueryResultResponseModel>> Execute(TemplateQueryExecuteModel query)
    {
        var queryExpression = new StringBuilder();
        IEnumerable<IPublishedContent> contents = BuildQuery(query, queryExpression);

        var timer = new Stopwatch();
        timer.Start();
        var results = contents.ToList();
        timer.Stop();

        var contentTypeIconsByKey = _contentTypeService
            .GetAll(results.Select(content => content.ContentType.Key).Distinct())
            .ToDictionary(contentType => contentType.Key, contentType => contentType.Icon);

        return await Task.FromResult(Ok(new TemplateQueryResultResponseModel
        {
            QueryExpression = queryExpression.ToString(),
            ResultCount = results.Count,
            ExecutionTime = timer.ElapsedMilliseconds,
            SampleResults = results.Take(20).Select(content => new TemplateQueryResultItemPresentationModel
            {
                Icon = contentTypeIconsByKey[content.ContentType.Key] ?? "icon-document",
                Name = content.Name
            })
        }));
    }

    private IEnumerable<IPublishedContent> BuildQuery(TemplateQueryExecuteModel model, StringBuilder queryExpression)
    {
        IPublishedContent? rootContent = GetRootContent(model, queryExpression);

        IEnumerable<IPublishedContent> contentQuery = GetRootContentQuery(model, rootContent, queryExpression);

        contentQuery = ApplyFiltering(model.Filters, contentQuery, queryExpression);

        contentQuery = ApplySorting(model.Sort, contentQuery, queryExpression);

        contentQuery = ApplyPaging(model.Take, contentQuery, queryExpression);

        return contentQuery;
    }

    private IPublishedContent? GetRootContent(TemplateQueryExecuteModel model, StringBuilder queryExpression)
    {
        IPublishedContent? rootContent;

        if (model.RootContentId != null && model.RootContentId != Guid.Empty)
        {
            rootContent = _publishedContentQuery.Content(model.RootContentId);
            queryExpression.Append($"Umbraco.Content(Guid.Parse(\"{model.RootContentId}\"))");
        }
        else
        {
            rootContent = _publishedContentQuery.ContentAtRoot().FirstOrDefault();
            queryExpression.Append("Umbraco.ContentAtRoot().FirstOrDefault()");
        }

        return rootContent;
    }

    private IEnumerable<IPublishedContent> GetRootContentQuery(TemplateQueryExecuteModel model, IPublishedContent? rootContent, StringBuilder queryExpression)
    {
        queryExpression.Append(_indent);

        if (model.ContentTypeAlias.IsNullOrWhiteSpace() == false)
        {
            queryExpression.Append($".ChildrenOfType(\"{model.ContentTypeAlias}\")");
            return rootContent == null
                ? Enumerable.Empty<IPublishedContent>()
                : rootContent.ChildrenOfType(_variationContextAccessor, model.ContentTypeAlias);
        }

        queryExpression.Append(".Children()");
        return rootContent == null
            ? Enumerable.Empty<IPublishedContent>()
            : rootContent.Children(_variationContextAccessor);
    }

    private IEnumerable<IPublishedContent> ApplyFiltering(IEnumerable<TemplateQueryExecuteFilterPresentationModel>? filters, IEnumerable<IPublishedContent> contentQuery, StringBuilder queryExpression)
    {
        if (filters is not null)
        {
            contentQuery = ApplyFilters(filters, contentQuery, queryExpression);
        }

        contentQuery = contentQuery.Where(x => x.IsVisible(_publishedValueFallback));
        queryExpression.Append(_indent);
        queryExpression.Append(".Where(x => x.IsVisible())");

        return contentQuery;
    }

    private IEnumerable<IPublishedContent> ApplyFilters(IEnumerable<TemplateQueryExecuteFilterPresentationModel> filters, IEnumerable<IPublishedContent> contentQuery, StringBuilder queryExpression)
    {
        var propertyTypeByAlias = GetProperties().ToDictionary(p => p.Alias, p => p.Type);

        string PropertyModelType(TemplateQueryPropertyType templateQueryPropertyType)
            => templateQueryPropertyType switch
            {
                TemplateQueryPropertyType.Integer => "int",
                TemplateQueryPropertyType.String => "string",
                TemplateQueryPropertyType.DateTime => "datetime",
                _ => throw new ArgumentOutOfRangeException(nameof(templateQueryPropertyType))
            };

        IEnumerable<QueryCondition> conditions = filters
            .Where(f => f.ConstraintValue.IsNullOrWhiteSpace() == false && propertyTypeByAlias.ContainsKey(f.PropertyAlias))
            .Select(f => new QueryCondition
            {
                Property = new PropertyModel
                {
                    Alias = f.PropertyAlias,
                    Type = PropertyModelType(propertyTypeByAlias[f.PropertyAlias])
                },
                ConstraintValue = f.ConstraintValue,
                Term = new OperatorTerm { Operator = f.Operator }
            });

        // apply filters
        foreach (QueryCondition condition in conditions)
        {
            //x is passed in as the parameter alias for the linq where statement clause
            Expression<Func<IPublishedContent, bool>> operation = condition.BuildCondition<IPublishedContent>("x");

            //for review - this uses a tonized query rather then the normal linq query.
            contentQuery = contentQuery.Where(operation.Compile());
            queryExpression.Append(_indent);
            queryExpression.Append($".Where({operation})");
        }

        return contentQuery;
    }

    private IEnumerable<IPublishedContent> ApplySorting(TemplateQueryExecuteSortModel? sorting, IEnumerable<IPublishedContent> contentQuery, StringBuilder queryExpression)
    {
        if (string.IsNullOrWhiteSpace(sorting?.PropertyAlias))
        {
            return contentQuery;
        }

        var ascending = sorting.Direction == "ascending";

        int SortById(IPublishedContent content) => content.Id;
        DateTime SortByCreateDate(IPublishedContent content) => content.CreateDate;
        DateTime SortByPublishDate(IPublishedContent content) => content.UpdateDate;
        string SortByName(IPublishedContent content) => content.Name;

        contentQuery = sorting.PropertyAlias.ToLowerInvariant() switch
        {
            "id" => ascending
                ? contentQuery.OrderBy(SortById)
                : contentQuery.OrderByDescending(SortById),
            "createDate" => ascending
                ? contentQuery.OrderBy(SortByCreateDate)
                : contentQuery.OrderByDescending(SortByCreateDate),
            "publishDate" => ascending
                ? contentQuery.OrderBy(SortByPublishDate)
                : contentQuery.OrderByDescending(SortByPublishDate),
            _ => ascending
                ? contentQuery.OrderBy(SortByName)
                : contentQuery.OrderByDescending(SortByName),
        };

        queryExpression.Append(_indent);
        queryExpression.Append(ascending
            ? $".OrderBy(x => x.{sorting.PropertyAlias})"
            : $".OrderByDescending(x => x.{sorting.PropertyAlias})");

        return contentQuery;
    }

    private IEnumerable<IPublishedContent> ApplyPaging(int take, IEnumerable<IPublishedContent> contentQuery, StringBuilder queryExpression)
    {
        if (take <= 0)
        {
            return contentQuery;
        }

        contentQuery = contentQuery.Take(take);
        queryExpression.Append(_indent);
        queryExpression.Append($".Take({take})");

        return contentQuery;
    }
}
