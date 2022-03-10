using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;

namespace Umbraco.Cms.Infrastructure.Templates
{
    public interface IHtmlMacroParameterParser
    {
        IEnumerable<UmbracoEntityReference> FindUmbracoEntityReferencesFromEmbeddedMacros(string text);

        IEnumerable<UmbracoEntityReference> FindUmbracoEntityReferencesFromGridControlMacros(IEnumerable<GridValue.GridControl> macroValues);
    }
}
