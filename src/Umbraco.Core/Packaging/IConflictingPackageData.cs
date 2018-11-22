using System.Collections.Generic;
using System.Xml.Linq;
using Umbraco.Core.Models;

namespace Umbraco.Core.Packaging
{
    internal interface IConflictingPackageData
    {
        IEnumerable<IFile> FindConflictingStylesheets(XElement stylesheetNotes);
        IEnumerable<ITemplate> FindConflictingTemplates(XElement templateNotes);
        IEnumerable<IMacro> FindConflictingMacros(XElement macroNodes);
    }
}