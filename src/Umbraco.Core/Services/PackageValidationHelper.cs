using System;
using System.Linq;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public class PackageValidationHelper : IPackageValidationHelper
    {
        private readonly IMacroService _macroService;
        private readonly IFileService _fileService;

        public PackageValidationHelper(IMacroService macroService,
            IFileService fileService)
        {
            if (fileService != null) _fileService = fileService;
            else throw new ArgumentNullException("fileService");
            if (macroService != null) _macroService = macroService;
            else throw new ArgumentNullException("macroService");
        }

        public bool StylesheetExists(string styleSheetName, out Stylesheet existingStyleSheet)
        {
            existingStyleSheet = _fileService.GetStylesheets(styleSheetName).SingleOrDefault();
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