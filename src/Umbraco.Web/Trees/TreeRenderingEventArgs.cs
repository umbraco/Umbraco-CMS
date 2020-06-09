using System;
using System.Net.Http.Formatting;

namespace Umbraco.Web.Trees
{
    // Migrated to .NET Core
    public class TreeRenderingEventArgs : EventArgs
    {
        public FormDataCollection QueryStrings { get; private set; }

        public TreeRenderingEventArgs(FormDataCollection queryStrings)
        {
            QueryStrings = queryStrings;
        }
    }
}
