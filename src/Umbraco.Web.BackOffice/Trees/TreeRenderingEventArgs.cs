using System;
using Microsoft.AspNetCore.Http;

namespace Umbraco.Cms.Web.BackOffice.Trees
{
    public class TreeRenderingEventArgs : EventArgs
    {
        public FormCollection QueryStrings { get; private set; }

        public TreeRenderingEventArgs(FormCollection queryStrings)
        {
            QueryStrings = queryStrings;
        }
    }
}
