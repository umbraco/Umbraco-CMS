using System.Collections.Generic;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Macros
{
    /// <summary>
    /// Renders a macro
    /// </summary>
    public interface IMacroRenderer
    {
        MacroContent Render(string macroAlias, IPublishedContent content, IDictionary<string, object> macroParams);
    }
}
