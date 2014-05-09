using System;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public class ConflictingPackageContentFinder : IConflictingPackageContentFinder
    {
        private readonly IMacroService _macroService;
        private readonly IFileService _fileService;

        public ConflictingPackageContentFinder(IMacroService macroService,
            IFileService fileService)
        {
            if (fileService != null) _fileService = fileService;
            else throw new ArgumentNullException("fileService");
            if (macroService != null) _macroService = macroService;
            else throw new ArgumentNullException("macroService");
        }


        public IStylesheet[] FindConflictingStylesheets(XElement stylesheetNotes)
        {
            if (string.Equals(Constants.Packaging.StylesheetsNodeName, stylesheetNotes.Name.LocalName) == false)
            {
                throw new ArgumentException("the root element must be \"" + Constants.Packaging.StylesheetsNodeName + "\"", "stylesheetNotes");
            }

            return stylesheetNotes.Elements(Constants.Packaging.StylesheetNodeName)
                .Select(n =>
                {
                    XElement xElement = n.Element(Constants.Packaging.NameNodeName);
                    if (xElement == null)
                    {
                        throw new ArgumentException("Missing \"" + Constants.Packaging.NameNodeName + "\" element",
                            "stylesheetNotes");
                    }

                    return _fileService.GetStylesheetByName(xElement.Value) as IStylesheet;
                })
                .Where(v => v != null).ToArray();
        }

        public ITemplate[] FindConflictingTemplates(XElement templateNotes)
        {
            if (string.Equals(Constants.Packaging.TemplatesNodeName, templateNotes.Name.LocalName) == false)
            {
                throw new ArgumentException("Node must be a \"" + Constants.Packaging.TemplatesNodeName + "\" node",
                    "templateNotes");
            }

            return templateNotes.Elements(Constants.Packaging.TemplateNodeName)
                .Select(n =>
                {
                    XElement xElement = n.Element(Constants.Packaging.AliasNodeNameCapital) ?? n.Element(Constants.Packaging.AliasNodeNameSmall);
                    if (xElement == null)
                    {
                        throw new ArgumentException("missing a \"" + Constants.Packaging.AliasNodeNameCapital + "\" element",
                            "templateNotes");
                    }

                    return _fileService.GetTemplate(xElement.Value);
                })
                .Where(v => v != null).ToArray();
        }

        public IMacro[] FindConflictingMacros(XElement macroNodes)
        {
            if (string.Equals(Constants.Packaging.MacrosNodeName, macroNodes.Name.LocalName) == false)
            {
                throw new ArgumentException("Node must be a \"" + Constants.Packaging.MacrosNodeName + "\" node",
                    "macroNodes");
            }
            
            return macroNodes.Elements(Constants.Packaging.MacroNodeName)
                .Select(n =>
                {
                    XElement xElement = n.Element(Constants.Packaging.AliasNodeNameSmall) ?? n.Element(Constants.Packaging.AliasNodeNameCapital);
                    if (xElement == null)
                    {
                        throw new ArgumentException(string.Format("missing a \"{0}\" element in {0} element", Constants.Packaging.AliasNodeNameSmall),
                            "macroNodes");
                    }

                    return _macroService.GetByAlias(xElement.Value);
                })
                .Where(v => v != null).ToArray();
        }

    }
}