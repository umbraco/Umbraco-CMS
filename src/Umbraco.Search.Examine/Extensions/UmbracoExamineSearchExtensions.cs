using System.Text.RegularExpressions;
using Examine;
using Examine.Search;
using Umbraco.Cms.Core.Search;
using Umbraco.Extensions;
using Umbraco.Search.Extensions;

namespace Umbraco.Search.Examine.Extensions;

public static class UmbracoExamineSearchExtensions
{


    /// <summary>
    ///     Returns all index fields that are culture specific (suffixed) or invariant
    /// </summary>
    /// <param name="index"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public static IEnumerable<string> GetCultureAndInvariantFields(this IUmbracoIndex index, string culture)
    {
        IEnumerable<string> allFields = index.GetFieldNames();

        foreach (var field in allFields)
        {
            Match match = UmbracoSearchExtensions._cultureIsoCodeFieldNameMatchExpression.Match(field);
            if (match.Success && culture.InvariantEquals(match.Groups["CultureName"].Value))
            {
                yield return field; // matches this culture field
            }
            else if (!match.Success)
            {
                yield return field; // matches no culture field (invariant)
            }
        }
    }

    public static IBooleanOperation Id(this IQuery query, int id)
    {
        IBooleanOperation? fieldQuery = query.Id(id.ToInvariantString());
        return fieldQuery;
    }

    /// <summary>
    ///     Query method to search on parent id
    /// </summary>
    /// <param name="query"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static IBooleanOperation ParentId(this IQuery query, int id)
    {
        IBooleanOperation? fieldQuery = query.Field("parentID", id);
        return fieldQuery;
    }

    /// <summary>
    ///     Query method to search on node name
    /// </summary>
    /// <param name="query"></param>
    /// <param name="nodeName"></param>
    /// <returns></returns>
    public static IBooleanOperation NodeName(this IQuery query, string nodeName)
    {
        IBooleanOperation? fieldQuery = query.Field(
            UmbracoSearchFieldNames.NodeNameFieldName,
            (IExamineValue)new ExamineValue(Examineness.Explicit, nodeName));
        return fieldQuery;
    }

    /// <summary>
    ///     Query method to search on node name
    /// </summary>
    /// <param name="query"></param>
    /// <param name="nodeName"></param>
    /// <returns></returns>
    public static IBooleanOperation NodeName(this IQuery query, IExamineValue nodeName)
    {
        IBooleanOperation? fieldQuery = query.Field(UmbracoSearchFieldNames.NodeNameFieldName, nodeName);
        return fieldQuery;
    }

    /// <summary>
    ///     Query method to search on node type alias
    /// </summary>
    /// <param name="query"></param>
    /// <param name="nodeTypeAlias"></param>
    /// <returns></returns>
    public static IBooleanOperation NodeTypeAlias(this IQuery query, string nodeTypeAlias)
    {
        IBooleanOperation? fieldQuery = query.Field(
            ExamineFieldNames.ItemTypeFieldName,
            (IExamineValue)new ExamineValue(Examineness.Explicit, nodeTypeAlias));
        return fieldQuery;
    }

    /// <summary>
    ///     Query method to search on node type alias
    /// </summary>
    /// <param name="query"></param>
    /// <param name="nodeTypeAlias"></param>
    /// <returns></returns>
    public static IBooleanOperation NodeTypeAlias(this IQuery query, IExamineValue nodeTypeAlias)
    {
        IBooleanOperation? fieldQuery = query.Field(ExamineFieldNames.ItemTypeFieldName, nodeTypeAlias);
        return fieldQuery;
    }
}
