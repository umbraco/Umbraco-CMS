using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Web.Security
{
    /// <summary>
    /// This is used specifically to assign a default 'app' to a particular section in order to validate the 
    /// currently logged in user's allowed applications
    /// </summary>
    /// <remarks>
    /// This relates to these issues:
    /// http://issues.umbraco.org/issue/U4-2021
    /// http://issues.umbraco.org/issue/U4-529
    /// 
    /// In order to fix these issues we need to pass in an 'app' parameter but since we don't want to break compatibility 
    /// we will create this mapping to map a 'default application' to a section action (like creating or deleting)
    /// </remarks>
    public static class LegacyDefaultAppMapping
    {
        /// <summary>
        /// Constructor that assigns all initial known mappings
        /// </summary>
        static LegacyDefaultAppMapping()
        {            
        }

        private static readonly ConcurrentDictionary<string, string> NodeTypeAliasMapping = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// Adds the default app mapping to the node type
        /// </summary>
        /// <param name="nodeType">The nodeType is the same nodeType found in the UI.xml</param>
        /// <param name="defaultApp">The default app associated with this nodeType if the 'app' parameter was not detected</param>
        public static void AddNodeTypeMappingForCreateDialog(string nodeType, string defaultApp)
        {
            NodeTypeAliasMapping.AddOrUpdate(nodeType, s => defaultApp, (s, s1) => defaultApp);
        }

        internal static string GetDefaultAppForCreateDialog(string nodeTypeAlias)
        {
            string app;
            return NodeTypeAliasMapping.TryGetValue(nodeTypeAlias, out app) ? app : null;
        }

    }
}
