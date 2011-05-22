using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.interfaces;

namespace umbraco.MacroEngines
{
    public interface IRazorLibrary
    {
        INode Node { get; }
    }
}
