using System;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Collections;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Web;
using Umbraco.Web.Cache;
using Umbraco.Core.IO;
using System.Web;
using Umbraco.Core.Models;
using Umbraco.Core.Xml;
using Umbraco.Web.Composing;
using Umbraco.Web.Macros;

namespace umbraco
{
    /// <summary>
    /// Holds methods for parsing and building umbraco templates
    /// </summary>
    [Obsolete("Do not use this class, use Umbraco.Core.Service.IFileService to work with templates")]
    public class template
    {
        #region private variables

        readonly StringBuilder _templateOutput = new StringBuilder();

        private string _templateDesign = "";
        int _masterTemplate = -1;
        private string _templateName = "";
        private string _templateAlias = "";

        #endregion

        #region public properties
        public string TemplateContent
        {
            set
            {
                _templateOutput.Append(value);
            }
            get
            {
                return _templateOutput.ToString();
            }
        }

        public int MasterTemplate
        {
            get { return _masterTemplate; }
        }

        //added fallback to the default template to avoid nasty .net errors.
        //This is referenced in /default.aspx.cs during page rendering.
        public string MasterPageFile
        {
            get
            {

                string file = TemplateAlias.Replace(" ", "") + ".master";
                string path = SystemDirectories.Masterpages + "/" + file;


                if (System.IO.File.Exists(IOHelper.MapPath(VirtualPathUtility.ToAbsolute(path))))
                    return path;
                else
                    return SystemDirectories.Umbraco + "/masterPages/default.master";
            }
        }

        //Support for template folders, if a alternative skin folder is requested
        //we will try to look for template files in another folder
        public string AlternateMasterPageFile(string templateFolder)
        {
            string file = TemplateAlias.Replace(" ", "") + ".master";
            string path = SystemDirectories.Masterpages + "/" + templateFolder + "/" + file;

            //if it doesn't exists then we return the normal file
            if (!System.IO.File.Exists(IOHelper.MapPath(VirtualPathUtility.ToAbsolute(path))))
            {

                string originalPath = IOHelper.MapPath(VirtualPathUtility.ToAbsolute(MasterPageFile));
                string copyPath = IOHelper.MapPath(VirtualPathUtility.ToAbsolute(path));

                string newFile;
                using (var fs = new FileStream(originalPath, FileMode.Open, FileAccess.ReadWrite))
                using (var f = new StreamReader(fs))
                {
                    newFile = f.ReadToEnd();
                }

                newFile = newFile.Replace("MasterPageFile=\"~/masterpages/", "MasterPageFile=\"");

                using (var fs = new FileStream(copyPath, FileMode.Create, FileAccess.Write))
                using (var replacement = new StreamWriter(fs))
                {
                    replacement.Write(newFile);
                }
            }

            return path;

        }


        public string TemplateAlias
        {
            get { return _templateAlias; }
        }
        #endregion

        #region public methods

        public override string ToString()
        {
            return this._templateName;
        }

        public Control parseStringBuilder(StringBuilder tempOutput, page umbPage)
        {

            Control pageContent = new Control();

            bool stop = false;
            bool debugMode = UmbracoContext.Current.HttpContext.IsDebuggingEnabled;

            while (!stop)
            {
                System.Web.HttpContext.Current.Trace.Write("template", "Begining of parsing rutine...");
                int tagIndex = tempOutput.ToString().ToLower().IndexOf("<?umbraco");
                if (tagIndex > -1)
                {
                    string tempElementContent = "";
                    pageContent.Controls.Add(new LiteralControl(tempOutput.ToString().Substring(0, tagIndex)));

                    tempOutput.Remove(0, tagIndex);

                    string tag = tempOutput.ToString().Substring(0, tempOutput.ToString().IndexOf(">") + 1);
                    Hashtable attributes = new Hashtable(XmlHelper.GetAttributesFromElement(tag));

                    // Check whether it's a single tag (<?.../>) or a tag with children (<?..>...</?...>)
                    if (tag.Substring(tag.Length - 2, 1) != "/" && tag.IndexOf(" ") > -1)
                    {
                        string closingTag = "</" + (tag.Substring(1, tag.IndexOf(" ") - 1)) + ">";
                        // Tag with children are only used when a macro is inserted by the umbraco-editor, in the
                        // following format: "<?UMBRACO_MACRO ...><IMG SRC="..."..></?UMBRACO_MACRO>", so we
                        // need to delete extra information inserted which is the image-tag and the closing
                        // umbraco_macro tag
                        if (tempOutput.ToString().IndexOf(closingTag) > -1)
                        {
                            tempOutput.Remove(0, tempOutput.ToString().IndexOf(closingTag));
                        }
                    }



                    System.Web.HttpContext.Current.Trace.Write("umbTemplate", "Outputting item: " + tag);

                    // Handle umbraco macro tags
                    if (tag.ToString().ToLower().IndexOf("umbraco_macro") > -1)
                    {
                        if (debugMode)
                            pageContent.Controls.Add(new LiteralControl("<div title=\"Macro Tag: '" + System.Web.HttpContext.Current.Server.HtmlEncode(tag) + "'\" style=\"border: 1px solid #009;\">"));

                        umbraco.presentation.templateControls.Macro macroControl = new umbraco.presentation.templateControls.Macro();
                        macroControl.Alias = helper.FindAttribute(attributes, "macroalias");
                        IDictionaryEnumerator ide = attributes.GetEnumerator();
                        while (ide.MoveNext())
                            if (macroControl.Attributes[ide.Key.ToString()] == null)
                                macroControl.Attributes.Add(ide.Key.ToString(), ide.Value.ToString());
                        pageContent.Controls.Add(macroControl);

                        if (debugMode)
                            pageContent.Controls.Add(new LiteralControl("</div>"));
                    }
                    else
                    {
                        if (tag.ToLower().IndexOf("umbraco_getitem") > -1)
                        {
                            umbraco.presentation.templateControls.Item itemControl = new umbraco.presentation.templateControls.Item();
                            itemControl.Field = helper.FindAttribute(attributes, "field");
                            IDictionaryEnumerator ide = attributes.GetEnumerator();
                            while (ide.MoveNext())
                                if (itemControl.Attributes[ide.Key.ToString()] == null)
                                    itemControl.Attributes.Add(ide.Key.ToString(), ide.Value.ToString());
                            pageContent.Controls.Add(itemControl);

                        }
                    }
                    tempOutput.Remove(0, tempOutput.ToString().IndexOf(">") + 1);
                    tempOutput.Insert(0, tempElementContent);
                }
                else
                {
                    pageContent.Controls.Add(new LiteralControl(tempOutput.ToString()));
                    break;
                }

            }

            return pageContent;

        }


        /// <summary>
        /// Parses the content of the templateOutput stringbuilder, and matches any tags given in the
        /// XML-file /umbraco/config/umbracoTemplateTags.xml.
        /// Replaces the found tags in the StringBuilder object, with "real content"
        /// </summary>
        /// <param name="umbPage"></param>
        public void Parse(page umbPage)
        {
            System.Web.HttpContext.Current.Trace.Write("umbracoTemplate", "Start parsing");

            // First parse for known umbraco tags
            // <?UMBRACO_MACRO/> - macros
            // <?UMBRACO_GETITEM/> - print item from page, level, or recursive
            MatchCollection tags = Regex.Matches(_templateOutput.ToString(), "<\\?UMBRACO_MACRO[^>]*/>|<\\?UMBRACO_GETITEM[^>]*/>|<\\?(?<tagName>[\\S]*)[^>]*/>", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

            foreach (Match tag in tags)
            {
                Hashtable attributes = new Hashtable(XmlHelper.GetAttributesFromElement(tag.Value));


                if (tag.ToString().ToLower().IndexOf("umbraco_macro") > -1)
                {
                    var macroId = helper.FindAttribute(attributes, "macroid");
                    if (macroId != "")
                        _templateOutput.Replace(tag.Value, string.Empty);
                }
                else
                {
                    if (tag.ToString().ToLower().IndexOf("umbraco_getitem") > -1)
                    {
                        try
                        {
                            var tempElementContent = umbPage.Elements[helper.FindAttribute(attributes, "field")].ToString();
                            var tempMacros = Regex.Matches(tempElementContent, "<\\?UMBRACO_MACRO(?<attributes>[^>]*)><img[^>]*><\\/\\?UMBRACO_MACRO>", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                            foreach (Match tempMacro in tempMacros)
                            {
                                var tempAttributes = new Hashtable(XmlHelper.GetAttributesFromElement(tempMacro.Groups["attributes"].Value));
                                var macroId = helper.FindAttribute(tempAttributes, "macroid");
                                if (Convert.ToInt32(macroId) > 0)
                                    _templateOutput.Replace(tag.Value, string.Empty);
                            }

                            _templateOutput.Replace(tag.Value, tempElementContent);
                        }
                        catch (Exception e)
                        {
                            System.Web.HttpContext.Current.Trace.Warn("umbracoTemplate", "Error reading element (" + helper.FindAttribute(attributes, "field") + ")", e);
                        }
                    }
                }
            }

            System.Web.HttpContext.Current.Trace.Write("umbracoTemplate", "Done parsing");
        }



        #endregion

        #region private methods

        private static MacroModel GetMacro(string macroId)
        {
            HttpContext.Current.Trace.Write("umbracoTemplate", "Starting macro (" + macroId + ")");
            // it's all obsolete anyways...
            var macro = Current.Services.MacroService.GetByAlias(macroId);
            return macro == null ? null : new MacroModel(macro);
        }

        #endregion

        #region constructors

        public static string GetMasterPageName(int templateID)
        {
            return GetMasterPageName(templateID, null);
        }

        public static string GetMasterPageName(int templateID, string templateFolder)
        {
            var t = new template(templateID);

            return !string.IsNullOrEmpty(templateFolder)
                ? t.AlternateMasterPageFile(templateFolder)
                : t.MasterPageFile;
        }

        public template(int templateID)
        {
            var tId = templateID;

            var t = Current.ApplicationCache.RuntimeCache.GetCacheItem<template>(
               string.Format("{0}{1}", CacheKeys.TemplateFrontEndCacheKey, tId), () =>
               {
                   dynamic templateData;
                   using (var scope = Current.ScopeProvider.CreateScope())
                   {
                       templateData = scope.Database.FirstOrDefault<dynamic>(
                           @"select nodeId, alias, node.parentID as master, text, design
from cmsTemplate
inner join umbracoNode node on (node.id = cmsTemplate.nodeId)
where nodeId = @templateID",
                           new { templateID = templateID });
                   }
                   if (templateData != null)
                   {
                       // Get template master and replace content where the template
                       if (templateData.master != null)
                           _masterTemplate = templateData.master;
                       if (templateData.alias != null)
                           _templateAlias = templateData.alias;
                       if (templateData.text != null)
                           _templateName = templateData.text;
                       if (templateData.design != null)
                           _templateDesign = templateData.design;
                   }

                   return this;
               });

            if (t == null)
                throw new InvalidOperationException("Could not find a tempalte with id " + templateID);

            this._masterTemplate = t._masterTemplate;
            this._templateAlias = t._templateAlias;
            this._templateDesign = t._templateDesign;
            this._masterTemplate = t._masterTemplate;
            this._templateName = t._templateName;

        }

        private void checkForMaster(int templateID) {
            // Get template design
            if (_masterTemplate != 0 && _masterTemplate != templateID) {
                template masterTemplateDesign = new template(_masterTemplate);
                if (masterTemplateDesign.TemplateContent.IndexOf("<?UMBRACO_TEMPLATE_LOAD_CHILD/>") > -1
                    || masterTemplateDesign.TemplateContent.IndexOf("<?UMBRACO_TEMPLATE_LOAD_CHILD />") > -1) {
                    _templateOutput.Append(
                        masterTemplateDesign.TemplateContent.Replace("<?UMBRACO_TEMPLATE_LOAD_CHILD/>",
                        _templateDesign).Replace("<?UMBRACO_TEMPLATE_LOAD_CHILD />", _templateDesign)
                        );
                } else
                    _templateOutput.Append(_templateDesign);
            } else {
                if (_masterTemplate == templateID)
                {
                    var t = Current.Services.FileService.GetTemplate(templateID);
                    var text = t.Name;
                    if (text.StartsWith("#"))
                    {
                        var lang = Current.Services.LocalizationService.GetLanguageByIsoCode(System.Threading.Thread.CurrentThread.CurrentCulture.Name);
                        if (lang != null && Current.Services.LocalizationService.DictionaryItemExists(text.Substring(1)))
                        {
                            var di = Current.Services.LocalizationService.GetDictionaryItemByKey(text.Substring(1));
                            text = di.GetTranslatedValue(lang.Id);
                        }
                        else
                        {
                            text = "[" + text + "]";
                        }
                    }
                    string templateName = (t != null) ? text : string.Format("'Template with id: '{0}", templateID);
                    System.Web.HttpContext.Current.Trace.Warn("template",
                        String.Format("Master template is the same as the current template. It would cause an endless loop! Make sure that the current template '{0}' has another Master Template than itself. You can change this in the template editor under 'Settings'", templateName));
                    _templateOutput.Append(_templateDesign);
                }
            }
        }

        [Obsolete("Use ApplicationContext.Current.ApplicationCache.ClearCacheForTemplate instead")]
        public static void ClearCachedTemplate(int templateID)
        {
            Current.DistributedCache.RefreshTemplateCache(templateID);
        }

        public template(string templateContent)
        {
            _templateOutput.Append(templateContent);
            _masterTemplate = 0;
        }

        #endregion
    }
}
