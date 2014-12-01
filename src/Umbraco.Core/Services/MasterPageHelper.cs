using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Umbraco.Core.IO;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    internal class MasterPageHelper
    {
        internal static readonly string DefaultMasterTemplate = SystemDirectories.Umbraco + "/masterpages/default.master";
        private static readonly char[] NewLineChars = Environment.NewLine.ToCharArray();

        internal static bool MasterPageExists(ITemplate t)
        {
            return System.IO.File.Exists(GetFilePath(t));
        }

        internal static string GetFilePath(ITemplate t)
        {
            return IOHelper.MapPath(SystemDirectories.Masterpages + "/" + t.Alias.Replace(" ", "") + ".master");
        }

        internal static string CreateMasterPage(ITemplate t, IFileService fileService, bool overWrite = false)
        {
            string masterpageContent = "";

            if (System.IO.File.Exists(GetFilePath(t)) == false || overWrite)
            {
                masterpageContent = CreateDefaultMasterPageContent(t, t.Alias, fileService);
                SaveDesignToFile(t, null, masterpageContent);
            }
            else
            {
                using (var tr = new StreamReader(GetFilePath(t)))
                {
                    masterpageContent = tr.ReadToEnd();
                    tr.Close();    
                }
            }

            return masterpageContent;
        }

        internal static string GetFileContents(ITemplate t)
        {
            string masterpageContent = "";
            if (System.IO.File.Exists(GetFilePath(t)))
            {
                using (var tr = new StreamReader(GetFilePath(t)))
                {
                    masterpageContent = tr.ReadToEnd();
                    tr.Close();    
                }
            }

            return masterpageContent;
        }

        internal static string UpdateMasterPageFile(ITemplate t, string currentAlias, IFileService fileService)
        {
            var template = UpdateMasterPageContent(t, currentAlias);
            UpdateChildTemplates(t, currentAlias, fileService);
            SaveDesignToFile(t, currentAlias, template);

            return template;
        }

        internal static string CreateDefaultMasterPageContent(ITemplate template, string currentAlias, IFileService fileService)
        {
            var design = new StringBuilder();
            design.Append(GetMasterPageHeader(template) + Environment.NewLine);

            if (template.MasterTemplateAlias.IsNullOrWhiteSpace() == false)
            {
                var master = fileService.GetTemplate(template.MasterTemplateAlias);
                if (master != null)
                {
                    foreach (var cpId in GetContentPlaceholderIds(master))
                    {
                        design.Append("<asp:content ContentPlaceHolderId=\"" + cpId + "\" runat=\"server\">" +
                                      Environment.NewLine +
                                      Environment.NewLine +
                                      "</asp:content>" +
                                      Environment.NewLine +
                                      Environment.NewLine);
                    }

                    return design.ToString();
                }
            }

            design.Append(GetMasterContentElement(template) + Environment.NewLine);
            design.Append(template.Content + Environment.NewLine);
            design.Append("</asp:Content>" + Environment.NewLine);

            return design.ToString();
        }

        internal static IEnumerable<string> GetContentPlaceholderIds(ITemplate template)
        {
            var retVal = new List<string>();

            var mp = template.Content;
            var path = "<asp:ContentPlaceHolder+(\\s+[a-zA-Z]+\\s*=\\s*(\"([^\"]*)\"|'([^']*)'))*\\s*/?>";
            var r = new Regex(path, RegexOptions.IgnoreCase);
            var m = r.Match(mp);

            while (m.Success)
            {
                var cc = m.Groups[3].Captures;
                retVal.AddRange(cc.Cast<Capture>().Where(c => c.Value != "server").Select(c => c.Value));

                m = m.NextMatch();
            }

            return retVal;
        }

        internal static string UpdateMasterPageContent(ITemplate template, string currentAlias)
        {
            var masterPageContent = template.Content;

            if (string.IsNullOrEmpty(currentAlias) == false && currentAlias != template.Alias)
            {
                var masterHeader =
                    masterPageContent.Substring(0, masterPageContent.IndexOf("%>", StringComparison.Ordinal) + 2).Trim(
                        Environment.NewLine.ToCharArray());

                // find the masterpagefile attribute
                var m = Regex.Matches(masterHeader, "(?<attributeName>\\S*)=\"(?<attributeValue>[^\"]*)\"",
                    RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

                foreach (Match attributeSet in m)
                {
                    if (attributeSet.Groups["attributeName"].Value.ToLower() == "masterpagefile")
                    {
                        // validate the masterpagefile
                        var currentMasterPageFile = attributeSet.Groups["attributeValue"].Value;
                        var currentMasterTemplateFile = ParentTemplatePath(template);

                        if (currentMasterPageFile != currentMasterTemplateFile)
                        {
                            masterPageContent =
                                masterPageContent.Replace(
                                    attributeSet.Groups["attributeName"].Value + "=\"" + currentMasterPageFile + "\"",
                                    attributeSet.Groups["attributeName"].Value + "=\"" + currentMasterTemplateFile +
                                    "\"");
                        }
                    }
                }
            }

            return masterPageContent;
        }

        private static void UpdateChildTemplates(ITemplate template, string currentAlias, IFileService fileService)
        {
            //if we have a Old Alias if the alias and therefor the masterpage file name has changed...
            //so before we save the new masterfile, we'll clear the old one, so we don't up with 
            //Unused masterpage files
            if (string.IsNullOrEmpty(currentAlias) == false && currentAlias != template.Alias)
            {
                //Ensure that child templates have the right master masterpage file name
                if (template.IsMasterTemplate)
                {
                    var children = fileService.GetTemplates(template.Id);
                    foreach (var t in children)
                        UpdateMasterPageFile(t, null, fileService);
                }
            }
        }


        private static void SaveDesignToFile(ITemplate t, string currentAlias, string design)
        {
            //kill the old file..
            if (string.IsNullOrEmpty(currentAlias) == false && currentAlias != t.Alias)
            {
                var oldFile =
                    IOHelper.MapPath(SystemDirectories.Masterpages + "/" + currentAlias.Replace(" ", "") + ".master");
                if (System.IO.File.Exists(oldFile))
                    System.IO.File.Delete(oldFile);
            }

            // save the file in UTF-8
            System.IO.File.WriteAllText(GetFilePath(t), design, Encoding.UTF8);
        }

        internal static void RemoveMasterPageFile(string alias)
        {
            if (string.IsNullOrWhiteSpace(alias) == false)
            {
                string file = IOHelper.MapPath(SystemDirectories.Masterpages + "/" + alias.Replace(" ", "") + ".master");
                if (System.IO.File.Exists(file))
                    System.IO.File.Delete(file);
            }
        }

        internal static string SaveTemplateToFile(ITemplate template, string currentAlias, IFileService fileService)
        {
            var masterPageContent = template.Content;
            if (IsMasterPageSyntax(masterPageContent) == false)
                masterPageContent = ConvertToMasterPageSyntax(template);

            // Add header to master page if it doesn't exist
            if (masterPageContent.TrimStart().StartsWith("<%@") == false)
            {
                masterPageContent = GetMasterPageHeader(template) + Environment.NewLine + masterPageContent;
            }
            else
            {
                // verify that the masterpage attribute is the same as the masterpage
                var masterHeader =
                    masterPageContent.Substring(0, masterPageContent.IndexOf("%>", StringComparison.Ordinal) + 2).Trim(NewLineChars);

                // find the masterpagefile attribute
                var m = Regex.Matches(masterHeader, "(?<attributeName>\\S*)=\"(?<attributeValue>[^\"]*)\"",
                    RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

                foreach (Match attributeSet in m)
                {
                    if (attributeSet.Groups["attributeName"].Value.ToLower() == "masterpagefile")
                    {
                        // validate the masterpagefile
                        var currentMasterPageFile = attributeSet.Groups["attributeValue"].Value;
                        var currentMasterTemplateFile = ParentTemplatePath(template);

                        if (currentMasterPageFile != currentMasterTemplateFile)
                        {
                            masterPageContent =
                                masterPageContent.Replace(
                                    attributeSet.Groups["attributeName"].Value + "=\"" + currentMasterPageFile + "\"",
                                    attributeSet.Groups["attributeName"].Value + "=\"" + currentMasterTemplateFile +
                                    "\"");

                        }
                    }
                }

            }

            //we have a Old Alias if the alias and therefor the masterpage file name has changed...
            //so before we save the new masterfile, we'll clear the old one, so we don't up with 
            //Unused masterpage files
            if (string.IsNullOrEmpty(currentAlias) == false && currentAlias != template.Alias)
            {

                //Ensure that child templates have the right master masterpage file name
                if (template.IsMasterTemplate)
                {
                    var children = fileService.GetTemplates(template.Id);

                    foreach (var t in children)
                        UpdateMasterPageFile(t, null, fileService);
                }

                //then kill the old file.. 
                var oldFile = IOHelper.MapPath(SystemDirectories.Masterpages + "/" + currentAlias.Replace(" ", "") + ".master");
                if (System.IO.File.Exists(oldFile))
                    System.IO.File.Delete(oldFile);
            }

            // save the file in UTF-8
            System.IO.File.WriteAllText(GetFilePath(template), masterPageContent, Encoding.UTF8);

            return masterPageContent;
        }

        internal static string ConvertToMasterPageSyntax(ITemplate template)
        {
            string masterPageContent = GetMasterContentElement(template) + Environment.NewLine;

            masterPageContent += template.Content;

            // Parse the design for getitems
            masterPageContent = EnsureMasterPageSyntax(template.Alias, masterPageContent);

            // append ending asp:content element
            masterPageContent += Environment.NewLine + "</asp:Content>" + Environment.NewLine;

            return masterPageContent;
        }

        internal static bool IsMasterPageSyntax(string code)
        {
            return Regex.IsMatch(code, @"<%@\s*Master", RegexOptions.IgnoreCase) ||
                   code.InvariantContains("<umbraco:Item") || code.InvariantContains("<asp:") || code.InvariantContains("<umbraco:Macro");
        }

        private static string GetMasterPageHeader(ITemplate template)
        {
            return String.Format("<%@ Master Language=\"C#\" MasterPageFile=\"{0}\" AutoEventWireup=\"true\" %>", ParentTemplatePath(template)) + Environment.NewLine;
        }

        private static string ParentTemplatePath(ITemplate template)
        {
            var masterTemplate = DefaultMasterTemplate;
            if (template.MasterTemplateAlias.IsNullOrWhiteSpace() == false)
                masterTemplate = SystemDirectories.Masterpages + "/" + template.MasterTemplateAlias + ".master";

            return masterTemplate;
        }

        private static string GetMasterContentElement(ITemplate template)
        {
            if (template.MasterTemplateAlias.IsNullOrWhiteSpace() == false)
            {
                string masterAlias = template.MasterTemplateAlias;
                return
                    String.Format("<asp:Content ContentPlaceHolderID=\"{0}ContentPlaceHolder\" runat=\"server\">", masterAlias);
            }
            else
                return
                    String.Format("<asp:Content ContentPlaceHolderID=\"ContentPlaceHolderDefault\" runat=\"server\">");

        }

        internal static string EnsureMasterPageSyntax(string templateAlias, string masterPageContent)
        {
            ReplaceElement(ref masterPageContent, "?UMBRACO_GETITEM", "umbraco:Item", true);
            ReplaceElement(ref masterPageContent, "?UMBRACO_GETITEM", "umbraco:Item", false);

            // Parse the design for macros
            ReplaceElement(ref masterPageContent, "?UMBRACO_MACRO", "umbraco:Macro", true);
            ReplaceElement(ref masterPageContent, "?UMBRACO_MACRO", "umbraco:Macro", false);

            // Parse the design for load childs
            masterPageContent = masterPageContent.Replace("<?UMBRACO_TEMPLATE_LOAD_CHILD/>", CreateDefaultPlaceHolder(templateAlias))
                .Replace("<?UMBRACO_TEMPLATE_LOAD_CHILD />", CreateDefaultPlaceHolder(templateAlias));
            // Parse the design for aspnet forms
            GetAspNetMasterPageForm(ref masterPageContent, templateAlias);
            masterPageContent = masterPageContent.Replace("</?ASPNET_FORM>", "</form>");
            // Parse the design for aspnet heads
            masterPageContent = masterPageContent.Replace("</ASPNET_HEAD>", String.Format("<head id=\"{0}Head\" runat=\"server\">", templateAlias.Replace(" ", "")));
            masterPageContent = masterPageContent.Replace("</?ASPNET_HEAD>", "</head>");
            return masterPageContent;
        }


        private static void GetAspNetMasterPageForm(ref string design, string templateAlias)
        {
            var formElement = Regex.Match(design, GetElementRegExp("?ASPNET_FORM", false), RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

            if (string.IsNullOrEmpty(formElement.Value) == false)
            {
                string formReplace = String.Format("<form id=\"{0}Form\" runat=\"server\">", templateAlias.Replace(" ", ""));
                if (formElement.Groups.Count == 0)
                {
                    formReplace += "<asp:scriptmanager runat=\"server\"></asp:scriptmanager>";
                }
                design = design.Replace(formElement.Value, formReplace);
            }
        }

        private static string CreateDefaultPlaceHolder(string templateAlias)
        {
            return String.Format("<asp:ContentPlaceHolder ID=\"{0}ContentPlaceHolder\" runat=\"server\"></asp:ContentPlaceHolder>", templateAlias.Replace(" ", ""));
        }

        private static void ReplaceElement(ref string design, string elementName, string newElementName, bool checkForQuotes)
        {
            var m =
                Regex.Matches(design, GetElementRegExp(elementName, checkForQuotes),
                    RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

            foreach (Match match in m)
            {
                GroupCollection groups = match.Groups;

                // generate new element (compensate for a closing trail on single elements ("/"))
                string elementAttributes = groups[1].Value;
                // test for macro alias
                if (elementName == "?UMBRACO_MACRO")
                {
                    var tags = XmlHelper.GetAttributesFromElement(match.Value);
                    if (tags["macroAlias"] != null)
                        elementAttributes = String.Format(" Alias=\"{0}\"", tags["macroAlias"]) + elementAttributes;
                    else if (tags["macroalias"] != null)
                        elementAttributes = String.Format(" Alias=\"{0}\"", tags["macroalias"]) + elementAttributes;
                }
                string newElement = "<" + newElementName + " runat=\"server\" " + elementAttributes.Trim() + ">";
                if (elementAttributes.EndsWith("/"))
                {
                    elementAttributes = elementAttributes.Substring(0, elementAttributes.Length - 1);
                }
                else if (groups[0].Value.StartsWith("</"))
                    // It's a closing element, so generate that instead of a starting element
                    newElement = "</" + newElementName + ">";

                if (checkForQuotes)
                {
                    // if it's inside quotes, we'll change element attribute quotes to single quotes
                    newElement = newElement.Replace("\"", "'");
                    newElement = String.Format("\"{0}\"", newElement);
                }
                design = design.Replace(match.Value, newElement);
            }
        }

        private static string GetElementRegExp(string elementName, bool checkForQuotes)
        {
            if (checkForQuotes)
                return String.Format("\"<[^>\\s]*\\b{0}(\\b[^>]*)>\"", elementName);
            else
                return String.Format("<[^>\\s]*\\b{0}(\\b[^>]*)>", elementName);

        }

    }
}