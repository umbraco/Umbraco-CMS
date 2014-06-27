using System.Xml.Linq;
using Umbraco.Core.Models;

namespace Umbraco.Core.Packaging
{
    internal interface IConflictingPackageContentFinder
    {
        IFile[] FindConflictingStylesheets(XElement stylesheetNotes);
        ITemplate[] FindConflictingTemplates(XElement templateNotes);
        IMacro[] FindConflictingMacros(XElement macroNodes);
    }
}