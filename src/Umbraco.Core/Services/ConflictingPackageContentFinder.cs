using System;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public class ConflictingPackageContentFinder : IConflictingPackageContentFinder
    {
        private readonly IMacroService _macroService;
        private readonly IFileService _fileService;

        public ConflictingPackageContentFinder(IMacroService macroService,
            IFileService fileService)
        {
            if (fileService != null) _fileService = fileService;
            else throw new ArgumentNullException("fileService");
            if (macroService != null) _macroService = macroService;
            else throw new ArgumentNullException("macroService");
        }

        public bool StylesheetExists(string styleSheetName, out IStylesheet existingStyleSheet)
        {
            existingStyleSheet = _fileService.GetStylesheetByName(styleSheetName);
            return existingStyleSheet != null;
        }

        public bool TemplateExists(string templateAlias, out ITemplate existingTemplate)
        {
            existingTemplate = _fileService.GetTemplate(templateAlias);
            return existingTemplate != null;
        }

        public bool MacroExists(string macroAlias, out IMacro existingMacro)
        {
            existingMacro = _macroService.GetByAlias(macroAlias);
            return macroAlias != null;
        }
    }
}