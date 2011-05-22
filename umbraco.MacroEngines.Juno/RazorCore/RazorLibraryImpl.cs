using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.interfaces;

namespace umbraco.MacroEngines
{
    public class RazorLibraryImpl : IRazorLibrary
    {
        private readonly INode _node;

        public RazorLibraryImpl(INode node)
        {
            _node = node;
        }
        public INode Node
        {
            get
            {
                return _node;
            }
        }
    }

}
