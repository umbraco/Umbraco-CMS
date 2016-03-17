using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core.Models;

namespace umbraco.cms.businesslogic.macro
{
    [Obsolete("Get rid of this!! move it and make it good")]
    public interface IMacroEngine 
    {
        string Name { get; }
        IEnumerable<string> SupportedExtensions { get; }
        IEnumerable<string> SupportedUIExtensions { get; }

        bool Validate(string code, string tempFileName, IPublishedContent currentPage, out string errorMessage);
        string Execute(MacroModel macro, IPublishedContent currentPage);
    }
}
