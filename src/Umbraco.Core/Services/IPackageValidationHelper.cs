using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public interface IPackageValidationHelper
    {
        bool StylesheetExists(string styleSheetName, out Stylesheet existingStyleSheet);
        bool TemplateExists(string templateAlias, out ITemplate existingTemplate);
        bool MacroExists(string macroAlias, out IMacro existingMacro);
    }
}