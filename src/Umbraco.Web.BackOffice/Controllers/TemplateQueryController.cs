using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Models.TemplateQuery;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Filters;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

/// <summary>
///     The API controller used for building content queries within the template
/// </summary>
[PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
[JsonCamelCaseFormatter]
public class TemplateQueryController : UmbracoAuthorizedJsonController
{
    private readonly IContentTypeService _contentTypeService;
    private readonly ILocalizedTextService _localizedTextService;
    private readonly IPublishedContentQuery _publishedContentQuery;
    private readonly IPublishedValueFallback _publishedValueFallback;
    private readonly IVariationContextAccessor _variationContextAccessor;

    public TemplateQueryController(
        IVariationContextAccessor variationContextAccessor,
        IPublishedContentQuery publishedContentQuery,
        ILocalizedTextService localizedTextService,
        IPublishedValueFallback publishedValueFallback,
        IContentTypeService contentTypeService)
    {
        _variationContextAccessor = variationContextAccessor ??
                                    throw new ArgumentNullException(nameof(variationContextAccessor));
        _publishedContentQuery =
            publishedContentQuery ?? throw new ArgumentNullException(nameof(publishedContentQuery));
        _localizedTextService =
            localizedTextService ?? throw new ArgumentNullException(nameof(localizedTextService));
        _publishedValueFallback =
            publishedValueFallback ?? throw new ArgumentNullException(nameof(publishedValueFallback));
        _contentTypeService = contentTypeService ?? throw new ArgumentNullException(nameof(contentTypeService));
    }

    private IEnumerable<OperatorTerm> Terms => new List<OperatorTerm>
    {
        new(_localizedTextService.Localize("template", "is"), Operator.Equals, new[] {"string"}),
        new(_localizedTextService.Localize("template", "isNot"), Operator.NotEquals, new[] {"string"}),
        new(_localizedTextService.Localize("template", "before"), Operator.LessThan, new[] {"datetime"}),
        new(_localizedTextService.Localize("template", "beforeIncDate"), Operator.LessThanEqualTo, new[] {"datetime"}),
        new(
            _localizedTextService.Localize("template", "after"),
            Operator.GreaterThan,
            new[]
            {
                "datetime"
            }),
        new(
            _localizedTextService.Localize("template", "afterIncDate"),
            Operator.GreaterThanEqualTo,
            new[]
            {
                "datetime"
            }),
        new(_localizedTextService.Localize("template", "equals"), Operator.Equals, new[] {"int"}),
        new(_localizedTextService.Localize("template", "doesNotEqual"), Operator.NotEquals, new[] {"int"}),
        new(_localizedTextService.Localize("template", "contains"), Operator.Contains, new[] {"string"}),
        new(_localizedTextService.Localize("template", "doesNotContain"), Operator.NotContains, new[] {"string"}),
        new(_localizedTextService.Localize("template", "greaterThan"), Operator.GreaterThan, new[] {"int"}),
        new(_localizedTextService.Localize("template", "greaterThanEqual"), Operator.GreaterThanEqualTo, new[] {"int"}),
        new(_localizedTextService.Localize("template", "lessThan"), Operator.LessThan, new[] {"int"}),
        new(_localizedTextService.Localize("template", "lessThanEqual"), Operator.LessThanEqualTo, new[] {"int"})
    };

    private IEnumerable<PropertyModel> Properties => new List<PropertyModel>
    {
        new() {Name = _localizedTextService.Localize("template", "id"), Alias = "Id", Type = "int"},
        new() {Name = _localizedTextService.Localize("template", "name"), Alias = "Name", Type = "string"},
        new()
        {
            Name = _localizedTextService.Localize("template", "createdDate"),
            Alias = "CreateDate",
            Type = "datetime"
        },
        new()
        {
            Name = _localizedTextService.Localize("template", "lastUpdatedDate"),
            Alias = "UpdateDate",
            Type = "datetime"
        }
    };

    public QueryResultModel PostTemplateQuery(QueryModel model)
    {
        var queryExpression = new StringBuilder();
        IEnumerable<IPublishedContent>? contents;

        if (model == null)
        {
            contents = _publishedContentQuery.ContentAtRoot().FirstOrDefault()?.Children(_variationContextAccessor);
            queryExpression.Append("Umbraco.ContentAtRoot().FirstOrDefault().Children()");
        }
        else
        {
            contents = PostTemplateValue(model, queryExpression);
        }

        // timing should be fairly correct, due to the fact that all the linq statements are yield returned.
        var timer = new Stopwatch();
        timer.Start();
        List<IPublishedContent> results = contents?.ToList() ?? new List<IPublishedContent>();
        timer.Stop();

        return new QueryResultModel
        {
            QueryExpression = queryExpression.ToString(),
            ResultCount = results.Count,
            ExecutionTime = timer.ElapsedMilliseconds,
            SampleResults = results.Take(20).Select(x => new TemplateQueryResult
            {
                Icon = "icon-document",
                Name = x.Name
            })
        };
    }

    private IEnumerable<IPublishedContent>? PostTemplateValue(QueryModel model, StringBuilder queryExpression)
    {
        var indent = Environment.NewLine + "    ";

        // set the source
        IPublishedContent? sourceDocument;
        if (model.Source != null && model.Source.Id > 0)
        {
            sourceDocument = _publishedContentQuery.Content(model.Source.Id);

            if (sourceDocument == null)
            {
                queryExpression.AppendFormat("Umbraco.Content({0})", model.Source.Id);
            }
            else
            {
                queryExpression.AppendFormat("Umbraco.Content(Guid.Parse(\"{0}\"))", sourceDocument.Key);
            }
        }
        else
        {
            sourceDocument = _publishedContentQuery.ContentAtRoot().FirstOrDefault();
            queryExpression.Append("Umbraco.ContentAtRoot().FirstOrDefault()");
        }

        // get children, optionally filtered by type
        IEnumerable<IPublishedContent>? contents;
        queryExpression.Append(indent);
        if (model.ContentType != null && !model.ContentType.Alias.IsNullOrWhiteSpace())
        {
            contents = sourceDocument == null
                ? Enumerable.Empty<IPublishedContent>()
                : sourceDocument.ChildrenOfType(_variationContextAccessor, model.ContentType.Alias);
            queryExpression.AppendFormat(".ChildrenOfType(\"{0}\")", model.ContentType.Alias);
        }
        else
        {
            contents = sourceDocument == null
                ? Enumerable.Empty<IPublishedContent>()
                : sourceDocument.Children(_variationContextAccessor);
            queryExpression.Append(".Children()");
        }

        if (model.Filters is not null)
        {
            // apply filters
            foreach (QueryCondition condition in model.Filters.Where(x => !x.ConstraintValue.IsNullOrWhiteSpace()))
            {
                //x is passed in as the parameter alias for the linq where statement clause
                Expression<Func<IPublishedContent, bool>> operation = condition.BuildCondition<IPublishedContent>("x");

                //for review - this uses a tonized query rather then the normal linq query.
                contents = contents?.Where(operation.Compile());
                queryExpression.Append(indent);
                queryExpression.AppendFormat(".Where({0})", operation);
            }
        }

        // always add IsVisible() to the query
        contents = contents?.Where(x => x.IsVisible(_publishedValueFallback));
        queryExpression.Append(indent);
        queryExpression.Append(".Where(x => x.IsVisible())");

        // apply sort
        if (model.Sort != null && (!model.Sort.Property?.Alias.IsNullOrWhiteSpace() ?? false))
        {
            contents = SortByDefaultPropertyValue(contents, model.Sort);

            queryExpression.Append(indent);
            queryExpression.AppendFormat(
                model.Sort.Direction == "ascending"
                    ? ".OrderBy(x => x.{0})"
                    : ".OrderByDescending(x => x.{0})"
                , model.Sort?.Property?.Alias);
        }

        // take
        if (model.Take > 0)
        {
            contents = contents?.Take(model.Take);
            queryExpression.Append(indent);
            queryExpression.AppendFormat(".Take({0})", model.Take);
        }

        return contents;
    }

    private object GetConstraintValue(QueryCondition condition)
    {
        switch (condition.Property.Type)
        {
            case "int":
                return int.Parse(condition.ConstraintValue, CultureInfo.InvariantCulture);
            case "datetime":
                DateTime dt;
                return DateTime.TryParse(condition.ConstraintValue, out dt) ? dt : DateTime.Today;
            default:
                return condition.ConstraintValue;
        }
    }

    private IEnumerable<IPublishedContent>? SortByDefaultPropertyValue(IEnumerable<IPublishedContent>? contents, SortExpression sortExpression)
    {
        switch (sortExpression.Property?.Alias)
        {
            case "id":
                return sortExpression.Direction == "ascending"
                    ? contents?.OrderBy(x => x.Id)
                    : contents?.OrderByDescending(x => x.Id);
            case "createDate":
                return sortExpression.Direction == "ascending"
                    ? contents?.OrderBy(x => x.CreateDate)
                    : contents?.OrderByDescending(x => x.CreateDate);
            case "publishDate":
                return sortExpression.Direction == "ascending"
                    ? contents?.OrderBy(x => x.UpdateDate)
                    : contents?.OrderByDescending(x => x.UpdateDate);
            case "name":
                return sortExpression.Direction == "ascending"
                    ? contents?.OrderBy(x => x.Name)
                    : contents?.OrderByDescending(x => x.Name);
            default:
                return sortExpression.Direction == "ascending"
                    ? contents?.OrderBy(x => x.Name)
                    : contents?.OrderByDescending(x => x.Name);
        }
    }

    /// <summary>
    ///     Gets a list of all content types
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ContentTypeModel> GetContentTypes()
    {
        var contentTypes = _contentTypeService.GetAll()
            .Select(x => new ContentTypeModel
            {
                Alias = x.Alias,
                Name = _localizedTextService.Localize("template", "contentOfType", new[] { x.Name ?? string.Empty })
            })
            .OrderBy(x => x.Name).ToList();

        contentTypes.Insert(
            0,
            new ContentTypeModel
            {
                Alias = string.Empty,
                Name = _localizedTextService.Localize("template", "allContent")
            });

        return contentTypes;
    }

    /// <summary>
    ///     Returns a collection of allowed properties.
    /// </summary>
    public IEnumerable<PropertyModel> GetAllowedProperties() => Properties.OrderBy(x => x.Name);

    /// <summary>
    ///     Returns a collection of constraint conditions that can be used in the query
    /// </summary>
    public IEnumerable<object> GetFilterConditions() => Terms;
}
