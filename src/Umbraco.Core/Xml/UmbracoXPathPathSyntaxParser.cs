using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Core.Xml
{
    /// <summary>
    /// This is used to parse our customize Umbraco XPath expressions (i.e. that include special tokens like $site) into 
    /// a real XPath statement
    /// </summary>
    internal class UmbracoXPathPathSyntaxParser
    {
        /// <summary>
        /// Parses custom umbraco xpath expression
        /// </summary>
        /// <param name="xpathExpression">The Xpath expression</param>
        /// <param name="nodeContextId">
        /// The current node id context of executing the query - null if there is no current node, in which case
        /// some of the parameters like $current, $parent, $site will be disabled
        /// </param>
        /// <param name="getPath">The callback to create the nodeId path, given a node Id</param>
        /// <param name="publishedContentExists">The callback to return whether a published node exists based on Id</param>
        /// <returns></returns>
        public static string ParseXPathQuery(
            string xpathExpression, 
            int? nodeContextId, 
            Func<int, IEnumerable<string>> getPath,
            Func<int, bool> publishedContentExists)
        {

            //TODO: This should probably support some of the old syntax and token replacements, currently 
            // it does not, there is a ticket raised here about it: http://issues.umbraco.org/issue/U4-6364
            // previous tokens were: "$currentPage", "$ancestorOrSelf", "$parentPage" and I beleive they were 
            // allowed 'inline', not just at the beginning... whether or not we want to support that is up 
            // for discussion.

            Mandate.ParameterNotNullOrEmpty(xpathExpression, "xpathExpression");
            Mandate.ParameterNotNull(getPath, "getPath");
            Mandate.ParameterNotNull(publishedContentExists, "publishedContentExists");

            //no need to parse it
            if (xpathExpression.StartsWith("$") == false)
                return xpathExpression;

            //get nearest published item
            Func<IEnumerable<string>, int> getClosestPublishedAncestor = (path =>
            {
                foreach (var i in path)
                {
                    int idAsInt;
                    if (int.TryParse(i, out idAsInt))
                    {
                        var exists = publishedContentExists(int.Parse(i));
                        if (exists)
                            return idAsInt;
                    }
                }
                return -1;
            });

            const string rootXpath = "descendant::*[@id={0}]";

            //parseable items:
            var vars = new Dictionary<string, Func<string, string>>();

            //These parameters must have a node id context
            if (nodeContextId.HasValue)
            {
                vars.Add("$current", q =>
                {
                    var closestPublishedAncestorId = getClosestPublishedAncestor(getPath(nodeContextId.Value));
                    return q.Replace("$current", string.Format(rootXpath, closestPublishedAncestorId));
                });

                vars.Add("$parent", q =>
                {
                    //remove the first item in the array if its the current node
                    //this happens when current is published, but we are looking for its parent specifically
                    var path = getPath(nodeContextId.Value).ToArray();
                    if (path[0] == nodeContextId.ToString())
                    {
                        path = path.Skip(1).ToArray();
                    }

                    var closestPublishedAncestorId = getClosestPublishedAncestor(path);
                    return q.Replace("$parent", string.Format(rootXpath, closestPublishedAncestorId));
                });


                vars.Add("$site", q =>
                {
                    var closestPublishedAncestorId = getClosestPublishedAncestor(getPath(nodeContextId.Value));
                    return q.Replace("$site", string.Format(rootXpath, closestPublishedAncestorId) + "/ancestor-or-self::*[@level = 1]");
                });
            }

            //TODO: This used to just replace $root with string.Empty BUT, that would never work
            // the root is always "/root . Need to confirm with Per why this was string.Empty before!
            vars.Add("$root", q => q.Replace("$root", "/root"));


            foreach (var varible in vars)
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
}
