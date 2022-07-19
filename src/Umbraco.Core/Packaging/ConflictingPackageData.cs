using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Packaging;

public class ConflictingPackageData
{
    private readonly IFileService _fileService;
    private readonly IMacroService _macroService;

    public ConflictingPackageData(IMacroService macroService, IFileService fileService)
    {
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _macroService = macroService ?? throw new ArgumentNullException(nameof(macroService));
    }

    public IEnumerable<IFile?>? FindConflictingStylesheets(IEnumerable<XElement>? stylesheetNodes) =>
        stylesheetNodes?
            .Select(n =>
            {
                XElement? xElement = n.Element("Name") ?? n.Element("name");
                if (xElement == null)
                {
                    throw new FormatException("Missing \"Name\" element");
                }

                return _fileService.GetStylesheet(xElement.Value) as IFile;
            })
            .Where(v => v != null);

    public IEnumerable<ITemplate>? FindConflictingTemplates(IEnumerable<XElement>? templateNodes) =>
        templateNodes?
            .Select(n =>
            {
                XElement? xElement = n.Element("Alias") ?? n.Element("alias");
                if (xElement == null)
                {
                    throw new FormatException("missing a \"Alias\" element");
                }

                return _fileService.GetTemplate(xElement.Value);
            })
            .WhereNotNull();

    public IEnumerable<IMacro?>? FindConflictingMacros(IEnumerable<XElement>? macroNodes) =>
        macroNodes?
            .Select(n =>
            {
                XElement? xElement = n.Element("alias") ?? n.Element("Alias");
                if (xElement == null)
                {
                    throw new FormatException("missing a \"alias\" element in alias element");
                }

                return _macroService.GetByAlias(xElement.Value);
            })
            .Where(v => v != null);
}
