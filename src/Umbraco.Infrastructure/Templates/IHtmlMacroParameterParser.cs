using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;

namespace Umbraco.Cms.Infrastructure.Templates;

/// <summary>
///     Provides methods to parse referenced entities as Macro parameters.
/// </summary>
public interface IHtmlMacroParameterParser
{
    /// <summary>
    ///     Parses out media UDIs from an HTML string based on embedded macro parameter values.
    /// </summary>
    /// <param name="text">HTML string</param>
    /// <returns></returns>
    IEnumerable<UmbracoEntityReference> FindUmbracoEntityReferencesFromEmbeddedMacros(string text);

    /// <summary>
    ///     Parses out media UDIs from Macro Grid Control parameters.
    /// </summary>
    /// <param name="macroGridControls"></param>
    /// <returns></returns>
    IEnumerable<UmbracoEntityReference> FindUmbracoEntityReferencesFromGridControlMacros(
        IEnumerable<GridValue.GridControl> macroGridControls);
}
