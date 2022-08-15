using System.Globalization;

namespace Umbraco.Cms.Core.Xml;

/// <summary>
///     This is used to parse our customize Umbraco XPath expressions (i.e. that include special tokens like $site) into
///     a real XPath statement
/// </summary>
public class UmbracoXPathPathSyntaxParser
{
    /// <summary>
    ///     Parses custom umbraco xpath expression
    /// </summary>
    /// <param name="xpathExpression">The Xpath expression</param>
    /// <param name="nodeContextId">
    ///     The current node id context of executing the query - null if there is no current node, in which case
    ///     some of the parameters like $current, $parent, $site will be disabled
    /// </param>
    /// <param name="getPath">The callback to create the nodeId path, given a node Id</param>
    /// <param name="publishedContentExists">The callback to return whether a published node exists based on Id</param>
    /// <returns></returns>
    public static string ParseXPathQuery(
        string xpathExpression,
        int? nodeContextId,
        Func<int, IEnumerable<string>?> getPath,
        Func<int, bool> publishedContentExists)
    {
        // TODO: This should probably support some of the old syntax and token replacements, currently
        // it does not, there is a ticket raised here about it: http://issues.umbraco.org/issue/U4-6364
        // previous tokens were: "$currentPage", "$ancestorOrSelf", "$parentPage" and I believe they were
        // allowed 'inline', not just at the beginning... whether or not we want to support that is up
        // for discussion.
        if (xpathExpression == null)
        {
            throw new ArgumentNullException(nameof(xpathExpression));
        }

        if (string.IsNullOrWhiteSpace(xpathExpression))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(xpathExpression));
        }

        if (getPath == null)
        {
            throw new ArgumentNullException(nameof(getPath));
        }

        if (publishedContentExists == null)
        {
            throw new ArgumentNullException(nameof(publishedContentExists));
        }

        // no need to parse it
        if (xpathExpression.StartsWith("$") == false)
        {
            return xpathExpression;
        }

        // get nearest published item
        Func<IEnumerable<string>?, int> getClosestPublishedAncestor = path =>
        {
            if (path is not null)
            {
                foreach (var i in path)
                {
                    if (int.TryParse(i, NumberStyles.Integer, CultureInfo.InvariantCulture, out int idAsInt))
                    {
                        var exists = publishedContentExists(int.Parse(i, CultureInfo.InvariantCulture));
                        if (exists)
                        {
                            return idAsInt;
                        }
                    }
                }
            }

            return -1;
        };

        const string rootXpath = "id({0})";

        // parseable items:
        var vars = new Dictionary<string, Func<string, string>>();

        // These parameters must have a node id context
        if (nodeContextId.HasValue)
        {
            vars.Add("$current", q =>
            {
                var closestPublishedAncestorId = getClosestPublishedAncestor(getPath(nodeContextId.Value));
                return q.Replace("$current", string.Format(rootXpath, closestPublishedAncestorId));
            });

            vars.Add("$parent", q =>
            {
                // remove the first item in the array if its the current node
                // this happens when current is published, but we are looking for its parent specifically
                var path = getPath(nodeContextId.Value)?.ToArray();
                if (path?[0] == nodeContextId.ToString())
                {
                    path = path?.Skip(1).ToArray();
                }

                var closestPublishedAncestorId = getClosestPublishedAncestor(path);
                return q.Replace("$parent", string.Format(rootXpath, closestPublishedAncestorId));
            });

            vars.Add("$site", q =>
            {
                var closestPublishedAncestorId = getClosestPublishedAncestor(getPath(nodeContextId.Value));
                return q.Replace(
                    "$site",
                    string.Format(rootXpath, closestPublishedAncestorId) + "/ancestor-or-self::*[@level = 1]");
            });
        }

        // TODO: This used to just replace $root with string.Empty BUT, that would never work
        // the root is always "/root . Need to confirm with Per why this was string.Empty before!
        vars.Add("$root", q => q.Replace("$root", "/root"));

        foreach (KeyValuePair<string, Func<string, string>> varible in vars)
        {
            if (xpathExpression.StartsWith(varible.Key))
            {
                xpathExpression = varible.Value(xpathExpression);
                break;
            }
        }

        return xpathExpression;
    }
}
