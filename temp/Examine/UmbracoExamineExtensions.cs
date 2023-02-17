using System.Text.RegularExpressions;
using Examine;
using Examine.Search;
using Umbraco.Cms.Infrastructure.Examine;

namespace Umbraco.Extensions;

public static class UmbracoExamineExtensions
{
    /// <summary>
    ///     Matches a culture iso name suffix
    /// </summary>
    /// <remarks>
    ///     myFieldName_en-us will match the "en-us"
    /// </remarks>
    internal static readonly Regex _cultureIsoCodeFieldNameMatchExpression = new(
        "^(?<FieldName>[_\\w]+)_(?<CultureName>[a-z]{2,3}(-[a-z0-9]{2,4})?)$",
        RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    // TODO: We need a public method here to just match a field name against CultureIsoCodeFieldNameMatchExpression

    /// <summary>
    ///     Returns all index fields that are culture specific (suffixed)
    /// </summary>
    /// <param name="index"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public static IEnumerable<string> GetCultureFields(this IUmbracoIndex index, string culture)
    {
        IEnumerable<string> allFields = index.GetFieldNames();

        var results = new List<string>();
        foreach (var field in allFields)
        {
            Match match = _cultureIsoCodeFieldNameMatchExpression.Match(field);
            if (match.Success && culture.InvariantEquals(match.Groups["CultureName"].Value))
            {
                results.Add(field);
            }
        }

        return results;
    }

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
            Match match = _cultureIsoCodeFieldNameMatchExpression.Match(field);
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
            UmbracoExamineFieldNames.NodeNameFieldName,
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
        IBooleanOperation? fieldQuery = query.Field(UmbracoExamineFieldNames.NodeNameFieldName, nodeName);
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
