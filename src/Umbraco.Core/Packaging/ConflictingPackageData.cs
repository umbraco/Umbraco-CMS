using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Core.Packaging
{
    internal class ConflictingPackageData
    {
        private readonly IMacroService _macroService;
        private readonly IFileService _fileService;

        public ConflictingPackageData(IMacroService macroService, IFileService fileService)
        {
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _macroService = macroService ?? throw new ArgumentNullException(nameof(macroService));
        }

        public IEnumerable<IFile> FindConflictingStylesheets(IEnumerable<XElement> stylesheetNodes)
        {
            return stylesheetNodes
                .Select(n =>
                {
                    var xElement = n.Element("Name") ?? n.Element("name");
                    if (xElement == null)
                        throw new FormatException("Missing \"Name\" element");

                    return _fileService.GetStylesheetByName(xElement.Value) as IFile;
                })
                .Where(v => v != null);
        }

        public IEnumerable<ITemplate> FindConflictingTemplates(IEnumerable<XElement> templateNodes)
        {
            return templateNodes
                .Select(n =>
                {
                    var xElement = n.Element("Alias") ?? n.Element("alias");
                    if (xElement == null)
                        throw new FormatException("missing a \"Alias\" element");

                    return _fileService.GetTemplate(xElement.Value);
                })
                .Where(v => v != null);
        }

        public IEnumerable<IMacro> FindConflictingMacros(IEnumerable<XElement> macroNodes)
        {
            return macroNodes
                .Select(n =>
                {
                    var xElement = n.Element("alias") ?? n.Element("Alias");
                    if (xElement == null)
                        throw new FormatException("missing a \"alias\" element in alias element");

                    return _macroService.GetByAlias(xElement.Value);
                })
                .Where(v => v != null);
        }
    }
}
