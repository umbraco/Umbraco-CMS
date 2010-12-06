using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.interfaces;

namespace umbraco.cms.businesslogic.macro
{
    public interface IMacroEngine
    {
        string Name
        {
            get;
        }
        List<string> SupportedExtensions
        {
            get;
        }
        Dictionary<string, IMacroGuiRendering> SupportedProperties
        {
            get;
        }

        bool Validate(string code, INode currentPage, out string errorMessage);
        string Execute(MacroModel macro, INode currentPage);
    }
}
