using System.Xml.Linq;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public interface IConflictingPackageContentFinder
    {

        IStylesheet[] FindConflictingStylesheets(XElement stylesheetNotes);
        ITemplate[] FindConflictingTemplates(XElement templateNotes);
        IMacro[] FindConflictingMacros(XElement macroNodes);
    }
}