using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Template.Query;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Models.TemplateQuery;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Template.Query;

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
    [ProducesResponseType(typeof(TemplateQueryResultViewModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<TemplateQueryResultViewModel>> Execute(TemplateQueryExecuteModel query)
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

        return await Task.FromResult(Ok(new TemplateQueryResultViewModel
        {
            QueryExpression = queryExpression.ToString(),
            ResultCount = results.Count,
            ExecutionTime = timer.ElapsedMilliseconds,
            SampleResults = results.Take(20).Select(content => new TemplateQueryResultItemViewModel
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

        contentQuery = ApplyFiltering(model, contentQuery, queryExpression);

        contentQuery = ApplySorting(model, contentQuery, queryExpression);

        contentQuery = ApplyPaging(model, contentQuery, queryExpression);

        return contentQuery;
    }

    private IPublishedContent? GetRootContent(TemplateQueryExecuteModel model, StringBuilder queryExpression)
    {
        IPublishedContent? rootContent;

        if (model.RootContentKey != null && model.RootContentKey != Guid.Empty)
        {
            rootContent = _publishedContentQuery.Content(model.RootContentKey);
            queryExpression.Append($"Umbraco.Content(Guid.Parse(\"{model.RootContentKey}\"))");
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

    private IEnumerable<IPublishedContent> ApplyFiltering(TemplateQueryExecuteModel model, IEnumerable<IPublishedContent> contentQuery, StringBuilder queryExpression)
    {
        if (model.Filters is not null)
        {
            var propertyTypeByAlias = GetProperties().ToDictionary(p => p.Alias, p => p.Type);

            IEnumerable<QueryCondition> conditions = model.Filters
                .Where(f => f.ConstraintValue.IsNullOrWhiteSpace() == false && propertyTypeByAlias.ContainsKey(f.PropertyAlias))
                .Select(f => new QueryCondition
                {
                    Property = new PropertyModel
                    {
                        Alias = f.PropertyAlias,
                        Type = propertyTypeByAlias[f.PropertyAlias]
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
        }

        contentQuery = contentQuery.Where(x => x.IsVisible(_publishedValueFallback));
        queryExpression.Append(_indent);
        queryExpression.Append(".Where(x => x.IsVisible())");

        return contentQuery;
    }

    private IEnumerable<IPublishedContent> ApplySorting(TemplateQueryExecuteModel model, IEnumerable<IPublishedContent> contentQuery, StringBuilder queryExpression)
    {
        if (model.Sort != null && model.Sort.PropertyAlias.IsNullOrWhiteSpace() == false)
        {
            var ascending = model.Sort.Direction == "ascending";
            contentQuery = model.Sort.PropertyAlias.ToLowerInvariant() switch
            {
                "id" => ascending
                    ? contentQuery.OrderBy(content => content.Id)
                    : contentQuery.OrderByDescending(content => content.Id),
                "createDate" => ascending
                    ? contentQuery.OrderBy(content => content.CreateDate)
                    : contentQuery.OrderByDescending(content => content.CreateDate),
                "publishDate" => ascending
                    ? contentQuery.OrderBy(content => content.UpdateDate)
                    : contentQuery.OrderByDescending(content => content.UpdateDate),
                _ => ascending
                    ? contentQuery.OrderBy(content => content.Name)
                    : contentQuery.OrderByDescending(content => content.Name),
            };

            queryExpression.Append(_indent);
            queryExpression.Append(ascending
                ? $".OrderBy(x => x.{model.Sort.PropertyAlias})"
                : $".OrderByDescending(x => x.{model.Sort.PropertyAlias})");
        }

        return contentQuery;
    }

    private IEnumerable<IPublishedContent> ApplyPaging(TemplateQueryExecuteModel model, IEnumerable<IPublishedContent> contentQuery, StringBuilder queryExpression)
    {
        if (model.Take > 0)
        {
            contentQuery = contentQuery.Take(model.Take);
            queryExpression.Append(_indent);
            queryExpression.Append($".Take({model.Take})");
        }

        return contentQuery;
    }
}
