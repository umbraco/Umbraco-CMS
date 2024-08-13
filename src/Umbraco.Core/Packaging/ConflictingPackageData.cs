using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Packaging;

public class ConflictingPackageData
{
    private readonly IFileService _fileService;

    public ConflictingPackageData(IFileService fileService)
        => _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));

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
}
