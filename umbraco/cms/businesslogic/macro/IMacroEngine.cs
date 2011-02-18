using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.interfaces;

namespace umbraco.cms.businesslogic.macro
{
    public interface IMacroEngine {
        string Name { get; }
        IEnumerable<string> SupportedExtensions { get; }
        IEnumerable<string> SupportedUIExtensions { get; }
        Dictionary<string, IMacroGuiRendering> SupportedProperties { get; }
        bool Validate(string code, string tempFileName, INode currentPage, out string errorMessage);
        string Execute(MacroModel macro, INode currentPage);
    }
}
