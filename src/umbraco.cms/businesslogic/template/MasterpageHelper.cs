using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Umbraco.Core.IO;

namespace umbraco.cms.businesslogic.template
{
    internal class MasterpageHelper
    {
        internal static readonly string DefaultMasterTemplate = SystemDirectories.Umbraco + "/masterpages/default.master";
        internal static string CreateMasterpageFile(Template t, bool overWrite = false)
        {
            string masterpageContent = "";

            if (!File.Exists(t.MasterPageFile) || overWrite)
                masterpageContent = saveTemplateToFile(t, t.Alias);
            else
            {
                System.IO.TextReader tr = new StreamReader(t.MasterPageFile);
                masterpageContent = tr.ReadToEnd();
                tr.Close();
            }

            return masterpageContent;
        }

        internal static string GetMasterpageFile(Template t)
        {
            string masterpageContent = "";
            if (File.Exists(t.MasterPageFile)){
                System.IO.TextReader tr = new StreamReader(t.MasterPageFile);
                masterpageContent = tr.ReadToEnd();
                tr.Close();
            }

            return masterpageContent;
        }

        internal static string UpdateMasterpageFile(Template t, string currentAlias)
        {
            return saveTemplateToFile(t, currentAlias);
        }

        internal static string saveTemplateToFile(Template template, string currentAlias)
        {
            var masterPageContent = template.Design;
            if (!isMasterPageSyntax(masterPageContent))
                masterPageContent = ConvertToMasterPageSyntax(template);

            // Add header to master page if it doesn't exist
            if (!masterPageContent.TrimStart().StartsWith("<%@"))
            {
                masterPageContent = getMasterPageHeader(template) + "\n" + masterPageContent;
            }
            else
            {
                // verify that the masterpage attribute is the same as the masterpage
                string masterHeader =
                    masterPageContent.Substring(0, masterPageContent.IndexOf("%>") + 2).Trim(
                        Environment.NewLine.ToCharArray());

                // find the masterpagefile attribute
                MatchCollection m = Regex.Matches(masterHeader, "(?<attributeName>\\S*)=\"(?<attributeValue>[^\"]*)\"",
                                                  RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                
                foreach (Match attributeSet in m)
                {
                    if (attributeSet.Groups["attributeName"].Value.ToLower() == "masterpagefile")
                    {
                        // validate the masterpagefile
                        string currentMasterPageFile = attributeSet.Groups["attributeValue"].Value;
                        string currentMasterTemplateFile = parentTemplatePath(template);

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
            if (!string.IsNullOrEmpty(currentAlias) && currentAlias != template.Alias)
            {

                //Ensure that child templates have the right master masterpage file name
                if (template.HasChildren)
                {
                    //store children array here because iterating over an Array property object is very inneficient.
                    var c = template.Children;
                    foreach (CMSNode cmn in c)
                        UpdateMasterpageFile(new Template(cmn.Id), null);
                }

                //then kill the old file.. 
                string _oldFile = IOHelper.MapPath(SystemDirectories.Masterpages + "/" + currentAlias.Replace(" ", "") + ".master");
                if (System.IO.File.Exists(_oldFile))
                    System.IO.File.Delete(_oldFile);
            }

            // save the file in UTF-8
            System.IO.File.WriteAllText(template.MasterPageFile, masterPageContent, System.Text.Encoding.UTF8);
            
            return masterPageContent;
        }

        internal static string ConvertToMasterPageSyntax(Template template)
        {
            string masterPageContent = GetMasterContentElement(template) + "\n";

            masterPageContent += template.Design;

            // Parse the design for getitems
            masterPageContent = EnsureMasterPageSyntax(template.Alias, masterPageContent);

            // append ending asp:content element
            masterPageContent += "\n</asp:Content>" + Environment.NewLine;

            return masterPageContent;
        }

        private static bool isMasterPageSyntax(string code)
        {
            return code.Contains("<%@ Master") || code.Contains("<umbraco:Item") || code.Contains("<asp:") || code.Contains("<umbraco:Macro");
        }

        private static string getMasterPageHeader(Template template)
        {
            return String.Format("<%@ Master Language=\"C#\" MasterPageFile=\"{0}\" AutoEventWireup=\"true\" %>", parentTemplatePath(template)) + Environment.NewLine;
        }

        private static string parentTemplatePath(Template template)
        {
            var masterTemplate = DefaultMasterTemplate;
            if (template.MasterTemplate != 0)
                masterTemplate = SystemDirectories.Masterpages + "/" + new Template(template.MasterTemplate).Alias.Replace(" ", "") + ".master";

            return masterTemplate;
        }

        private static string GetMasterContentElement(Template template)
        {
            if (template.MasterTemplate != 0)
            {
                string masterAlias = new Template(template.MasterTemplate).Alias.Replace(" ", "");
                return
                    String.Format("<asp:Content ContentPlaceHolderID=\"{1}ContentPlaceHolder\" runat=\"server\">",
                    template.Alias.Replace(" ", ""), masterAlias);
            }
            else
                return
                    String.Format("<asp:Content ContentPlaceHolderID=\"ContentPlaceHolderDefault\" runat=\"server\">",
                    template.Alias.Replace(" ", ""));

        }

        internal static string EnsureMasterPageSyntax(string templateAlias, string masterPageContent)
        {
            replaceElement(ref masterPageContent, "?UMBRACO_GETITEM", "umbraco:Item", true);
            replaceElement(ref masterPageContent, "?UMBRACO_GETITEM", "umbraco:Item", false);

            // Parse the design for macros
            replaceElement(ref masterPageContent, "?UMBRACO_MACRO", "umbraco:Macro", true);
            replaceElement(ref masterPageContent, "?UMBRACO_MACRO", "umbraco:Macro", false);

            // Parse the design for load childs
            masterPageContent = masterPageContent.Replace("<?UMBRACO_TEMPLATE_LOAD_CHILD/>",  createDefaultPlaceHolder(templateAlias))
                                                 .Replace("<?UMBRACO_TEMPLATE_LOAD_CHILD />", createDefaultPlaceHolder(templateAlias));
            // Parse the design for aspnet forms
            getAspNetMasterPageForm(ref masterPageContent, templateAlias);
            masterPageContent = masterPageContent.Replace("</?ASPNET_FORM>", "</form>");
            // Parse the design for aspnet heads
            masterPageContent = masterPageContent.Replace("</ASPNET_HEAD>", String.Format("<head id=\"{0}Head\" runat=\"server\">", templateAlias.Replace(" ", "")));
            masterPageContent = masterPageContent.Replace("</?ASPNET_HEAD>", "</head>");
            return masterPageContent;
        }


        private static void getAspNetMasterPageForm(ref string design, string templateAlias)
        {
            Match formElement = Regex.Match(design, getElementRegExp("?ASPNET_FORM", false), RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            
            if (formElement != null && formElement.Value != "")
            {
                string formReplace = String.Format("<form id=\"{0}Form\" runat=\"server\">", templateAlias.Replace(" ", ""));
                if (formElement.Groups.Count == 0)
                {
                    formReplace += "<asp:scriptmanager runat=\"server\"></asp:scriptmanager>";
                }
                design = design.Replace(formElement.Value, formReplace);
            }
        }

        private static string createDefaultPlaceHolder(string templateAlias)
        {
            return String.Format("<asp:ContentPlaceHolder ID=\"{0}ContentPlaceHolder\" runat=\"server\"></asp:ContentPlaceHolder>", templateAlias.Replace(" ", ""));
        }

        private static void replaceElement(ref string design, string elementName, string newElementName, bool checkForQuotes)
        {
            MatchCollection m =
                Regex.Matches(design, getElementRegExp(elementName, checkForQuotes),
                  RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

            foreach (Match match in m)
            {
                GroupCollection groups = match.Groups;

                // generate new element (compensate for a closing trail on single elements ("/"))
                string elementAttributes = groups[1].Value;
                // test for macro alias
                if (elementName == "?UMBRACO_MACRO")
                {
                    Hashtable tags = helpers.xhtml.ReturnAttributes(match.Value);
                    if (tags["macroAlias"] != null)
                        elementAttributes = String.Format(" Alias=\"{0}\"", tags["macroAlias"].ToString()) + elementAttributes;
                    else if (tags["macroalias"] != null)
                        elementAttributes = String.Format(" Alias=\"{0}\"", tags["macroalias"].ToString()) + elementAttributes;
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

        private static string getElementRegExp(string elementName, bool checkForQuotes)
        {
            if (checkForQuotes)
                return String.Format("\"<[^>\\s]*\\b{0}(\\b[^>]*)>\"", elementName);
            else
                return String.Format("<[^>\\s]*\\b{0}(\\b[^>]*)>", elementName);

        }

    }
}
