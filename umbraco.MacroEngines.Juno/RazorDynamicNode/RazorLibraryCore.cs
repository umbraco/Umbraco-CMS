using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.interfaces;

namespace umbraco.MacroEngines.Library
{
    public class RazorLibraryCore
    {
        private INode _node;
        public INode Node
        {
            get { return _node; }
        }
        public RazorLibraryCore(INode node)
        {
            this._node = node;
        }



        public string Helper1(string defaultParam1 = "")
        {
            return defaultParam1;
        }


    }
}
