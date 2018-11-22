using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.interfaces;

namespace umbraco.cms.businesslogic.macro
{
    public interface IMacroEngine : IDiscoverable
    {
        string Name { get; }
        IEnumerable<string> SupportedExtensions { get; }
        IEnumerable<string> SupportedUIExtensions { get; }

		[Obsolete("This property is not used in the codebase")]
        Dictionary<string, IMacroGuiRendering> SupportedProperties { get; }

        bool Validate(string code, string tempFileName, INode currentPage, out string errorMessage);
        string Execute(MacroModel macro, INode currentPage);
    }
}
