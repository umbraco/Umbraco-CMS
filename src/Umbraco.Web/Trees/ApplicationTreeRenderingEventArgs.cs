using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Web.Models.Trees;

namespace Umbraco.Web.Trees
{
    public class ApplicationTreeRenderingEventArgs : EventArgs
    {
        public string Application { get; }
        public string Tree { get; }
        public FormDataCollection QueryStrings { get; }
        public SectionRootNode SectionRootNode { get; }

        public ApplicationTreeRenderingEventArgs(string application, string tree, FormDataCollection queryStrings, SectionRootNode root)
        {
            Application = application;
            Tree = tree;
            QueryStrings = queryStrings;
            SectionRootNode = root;
        }
    }
}
