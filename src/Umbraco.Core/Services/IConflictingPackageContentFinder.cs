using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public interface IConflictingPackageContentFinder
    {
        bool StylesheetExists(string styleSheetName, out IStylesheet existingStylesheet);
        bool TemplateExists(string templateAlias, out ITemplate existingTemplate);
        bool MacroExists(string macroAlias, out IMacro existingMacro);
    }
}