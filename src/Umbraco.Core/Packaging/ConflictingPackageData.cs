using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Packaging;

/// <summary>
///     Provides methods to detect conflicting package data during package installation.
/// </summary>
public class ConflictingPackageData
{
    private readonly IStylesheetService _stylesheetService;
    private readonly ITemplateService _templateService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConflictingPackageData"/> class.
    /// </summary>
    /// <param name="stylesheetService">The stylesheet service used to check for existing stylesheets.</param>
    /// <param name="templateService">The template service used to check for existing templates.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stylesheetService"/> or <paramref name="templateService"/> is <c>null</c>.</exception>
    public ConflictingPackageData(IStylesheetService stylesheetService, ITemplateService templateService)
    {
        _stylesheetService = stylesheetService ?? throw new ArgumentNullException(nameof(stylesheetService));
        _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
    }

    /// <summary>
    ///     Finds stylesheets in the package that already exist in the system.
    /// </summary>
    /// <param name="stylesheetNodes">The XML elements representing stylesheets from the package.</param>
    /// <returns>A collection of existing stylesheets that conflict with the package, or <c>null</c> if no nodes provided.</returns>
    /// <exception cref="FormatException">Thrown when a stylesheet node is missing the required "Name" element.</exception>
    public IEnumerable<IFile?>? FindConflictingStylesheets(IEnumerable<XElement>? stylesheetNodes) =>
        stylesheetNodes?
            .Select(n =>
            {
                XElement? xElement = n.Element("Name") ?? n.Element("name");
                if (xElement == null)
                {
                    throw new FormatException("Missing \"Name\" element");
                }

                return _stylesheetService.GetAsync(xElement.Value).GetAwaiter().GetResult() as IFile;
            })
            .Where(v => v != null);

    /// <summary>
    ///     Finds templates in the package that already exist in the system.
    /// </summary>
    /// <param name="templateNodes">The XML elements representing templates from the package.</param>
    /// <returns>A collection of existing templates that conflict with the package, or <c>null</c> if no nodes provided.</returns>
    /// <exception cref="FormatException">Thrown when a template node is missing the required "Alias" element.</exception>
    public IEnumerable<ITemplate>? FindConflictingTemplates(IEnumerable<XElement>? templateNodes) =>
        templateNodes?
            .Select(n =>
            {
                XElement? xElement = n.Element("Alias") ?? n.Element("alias");
                if (xElement == null)
                {
                    throw new FormatException("missing a \"Alias\" element");
                }

                return _templateService.GetAsync(xElement.Value).GetAwaiter().GetResult();
            })
            .WhereNotNull();
}
