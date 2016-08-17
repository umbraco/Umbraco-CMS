using System;
using System.Linq;
using System.Collections;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using umbraco.cms.businesslogic.web;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Xml;

namespace umbraco.cms.businesslogic.template
{
    /// <summary>
    /// Summary description for Template.
    /// </summary>
    [Obsolete("Obsolete, Use IFileService and ITemplate to work with templates instead")]
    public class Template : CMSNode
    {
        
        #region Private members

        private readonly ViewHelper _viewHelper = new ViewHelper(new PhysicalFileSystem(SystemDirectories.MvcViews));
        private readonly MasterPageHelper _masterPageHelper = new MasterPageHelper(new PhysicalFileSystem(SystemDirectories.Masterpages));
        internal ITemplate TemplateEntity;        
        private int? _mastertemplate;
        
        #endregion

        #region Static members

        public static readonly string UmbracoMasterTemplate = SystemDirectories.Umbraco + "/masterpages/default.master";
        private static Hashtable _templateAliases = new Hashtable();      

        #endregion		


        [Obsolete("This is not used at all, do not use this")]
        public static Hashtable TemplateAliases
        {
            get { return _templateAliases; }
            set { _templateAliases = value; }
        }

        #region Constructors

        internal Template(ITemplate template)
            : base(template)
        {
            TemplateEntity = template;
        }

        public Template(int id) : base(id) { }

        public Template(Guid id) : base(id) { }

        #endregion

        /// <summary>
        /// Used to persist object changes to the database. In Version3.0 it's just a stub for future compatibility
        /// </summary>
        public override void Save()
        {
            ApplicationContext.Current.Services.FileService.SaveTemplate(TemplateEntity);
        }

        public string GetRawText()
        {
            return TemplateEntity.Name;
            //return base.Text;
        }

        //TODO: This is the name of the template, which can apparenlty be localized using the umbraco dictionary, so we need to cater for this but that
        // shoud really be done as part of mapping logic for models that are being consumed in the UI, not at the business logic layer.
        public override string Text
        {
            get
            {
                var tempText = TemplateEntity.Name;
                //string tempText = base.Text;
                if (!tempText.StartsWith("#"))
                    return tempText;
                else
                {
                    language.Language lang = language.Language.GetByCultureCode(System.Threading.Thread.CurrentThread.CurrentCulture.Name);
                    if (lang != null)
                    {                        
                        if (ApplicationContext.Current.Services.LocalizationService.DictionaryItemExists(tempText.Substring(1, tempText.Length - 1)))
                        {
                            var di = ApplicationContext.Current.Services.LocalizationService.GetDictionaryItemByKey(tempText.Substring(1, tempText.Length - 1));
                            return di.GetTranslatedValue(lang.id);
                        }
                    }

                    return "[" + tempText + "]";
                }
            }
            set
            {
                FlushCache();
                TemplateEntity.Name = value;
            }
        }

        [Obsolete("This is not used whatsoever")]
        public string OutputContentType { get; set; }

        protected override void setupNode()
        {
            TemplateEntity = ApplicationContext.Current.Services.FileService.GetTemplate(Id);
            if (TemplateEntity == null)
            {
                throw new ArgumentException(string.Format("No node exists with id '{0}'", Id));
            }
        }
		
        public new string Path
        {
            get
            {
                return TemplateEntity.Path;
            }
            set
            {
                TemplateEntity.Path = value;
            }
        }

        public string Alias
        {
            get { return TemplateEntity.Alias; }
            set { TemplateEntity.Alias = value; }

        }

        public bool HasMasterTemplate
        {
            get { return (_mastertemplate > 0); }
        }


        public override bool HasChildren
        {
            get { return TemplateEntity.IsMasterTemplate; }
            set
            {
                //Do nothing!
            }
        }

        public int MasterTemplate
        {
            get
            {
                if (_mastertemplate.HasValue == false)
                {
                    var master = ApplicationContext.Current.Services.FileService.GetTemplate(TemplateEntity.MasterTemplateAlias);
                    if (master != null)
                    {
                        _mastertemplate = master.Id;
                    }
                    else
                    {
                        _mastertemplate = -1;
                    }
                }
                return _mastertemplate.Value;
            }
            set
            {
                //set to null if it's zero                
                if (value == 0)
                {
                    TemplateEntity.SetMasterTemplate(null);
                }
                else
                {
                    var found = ApplicationContext.Current.Services.FileService.GetTemplate(value);
                    if (found != null)
                    {
                        TemplateEntity.SetMasterTemplate(found);
                        _mastertemplate = found.Id;
                    }
                }
            }
        }

        public string Design
        {
            get
            {
                return TemplateEntity.Content;
            }
            set
            {
                TemplateEntity.Content = value;
            }
        }

        public XmlNode ToXml(XmlDocument doc)
        {
            var serializer = new EntityXmlSerializer();
            var serialized = serializer.Serialize(TemplateEntity);
            return serialized.GetXmlNode(doc);
        }

        /// <summary>
        /// Removes any references to this templates from child templates, documenttypes and documents
        /// </summary>
        public void RemoveAllReferences()
        {
            if (HasChildren)
            {
                foreach (var t in GetAllAsList().FindAll(t => t.MasterTemplate == Id))
                {
                    t.MasterTemplate = 0;
                    t.Save();
                }
            }

            RemoveFromDocumentTypes();

            // remove from documents
            Document.RemoveTemplateFromDocument(this.Id);

            //save it
            Save();
        }

        public void RemoveFromDocumentTypes()
        {
            foreach (DocumentType dt in DocumentType.GetAllAsList().Where(x => x.allowedTemplates.Select(t => t.Id).Contains(this.Id)))
            {
                dt.RemoveTemplate(this.Id);
                dt.Save();
            }
        }

        [Obsolete("This method should have never existed here")]
        public IEnumerable<DocumentType> GetDocumentTypes()
        {
            return DocumentType.GetAllAsList().Where(x => x.allowedTemplates.Select(t => t.Id).Contains(this.Id));
        }

        public static Template MakeNew(string Name, IUser u, Template master)
        {
            return MakeNew(Name, u, master, null);
        }
        
        private static Template MakeNew(string name, IUser u, string design)
        {
            return MakeNew(name, u, null, design);
        }

        public static Template MakeNew(string name, IUser u)
        {
            return MakeNew(name, u, design: null);
        }

        private static Template MakeNew(string name, IUser u, Template master, string design)
        {
            var foundMaster = master == null ? null : ApplicationContext.Current.Services.FileService.GetTemplate(master.Id);
            var template = ApplicationContext.Current.Services.FileService.CreateTemplateWithIdentity(name, design, foundMaster, u.Id);

            var legacyTemplate = new Template(template);

            return legacyTemplate;
        }

        public static Template GetByAlias(string Alias)
        {
            return GetByAlias(Alias, false);
        }

        [Obsolete("this overload is the same as the other one, the useCache has no affect")]
        public static Template GetByAlias(string Alias, bool useCache)
        {
            var found = ApplicationContext.Current.Services.FileService.GetTemplate(Alias);
            return found == null ? null : new Template(found);
        }

        [Obsolete("Obsolete, please use GetAllAsList() method instead", true)]
        public static Template[] getAll()
        {
            return GetAllAsList().ToArray();
        }

        public static List<Template> GetAllAsList()
        {
            return ApplicationContext.Current.Services.FileService.GetTemplates().Select(x => new Template(x)).ToList();
        }

        public static int GetTemplateIdFromAlias(string alias)
        {
            var found = ApplicationContext.Current.Services.FileService.GetTemplate(alias);
            return found == null ? -1 : found.Id;
        }


        public override void delete()
        {
            // don't allow template deletion if it has child templates
            if (this.HasChildren)
            {                
                var ex = new InvalidOperationException("Can't delete a master template. Remove any bindings from child templates first.");
				LogHelper.Error<Template>("Can't delete a master template. Remove any bindings from child templates first.", ex);
	            throw ex;
            }


            //remove refs from documents
            SqlHelper.ExecuteNonQuery("UPDATE cmsDocument SET templateId = NULL WHERE templateId = " + this.Id);

                
            ApplicationContext.Current.Services.FileService.DeleteTemplate(TemplateEntity.Alias);

        }

        [Obsolete("This method, doesnt actually do anything, as the file is created when the design is set", false)]
        public void _SaveAsMasterPage()
        {

        }

        public string GetMasterContentElement(int masterTemplateId)
        {
            if (masterTemplateId != 0)
            {
                string masterAlias = new Template(masterTemplateId).Alias.Replace(" ", "");
                return
                    String.Format("<asp:Content ContentPlaceHolderID=\"{1}ContentPlaceHolder\" runat=\"server\">",
                    Alias.Replace(" ", ""), masterAlias);
            }
            else
                return
                    String.Format("<asp:Content ContentPlaceHolderID=\"ContentPlaceHolderDefault\" runat=\"server\">",
                    Alias.Replace(" ", ""));
        }

        public List<string> contentPlaceholderIds()
        {
            return MasterPageHelper.GetContentPlaceholderIds(TemplateEntity).ToList();
        }



        public string ConvertToMasterPageSyntax(string templateDesign)
        {
            string masterPageContent = GetMasterContentElement(MasterTemplate) + Environment.NewLine;

            masterPageContent += templateDesign;

            // Parse the design for getitems
            masterPageContent = EnsureMasterPageSyntax(masterPageContent);

            // append ending asp:content element
            masterPageContent += Environment.NewLine
                + "</asp:Content>" 
                + Environment.NewLine;

            return masterPageContent;
        }

        public string EnsureMasterPageSyntax(string masterPageContent)
        {
            ReplaceElement(ref masterPageContent, "?UMBRACO_GETITEM", "umbraco:Item", true);
            ReplaceElement(ref masterPageContent, "?UMBRACO_GETITEM", "umbraco:Item", false);

            // Parse the design for macros
            ReplaceElement(ref masterPageContent, "?UMBRACO_MACRO", "umbraco:Macro", true);
            ReplaceElement(ref masterPageContent, "?UMBRACO_MACRO", "umbraco:Macro", false);

            // Parse the design for load childs
            masterPageContent = masterPageContent.Replace("<?UMBRACO_TEMPLATE_LOAD_CHILD/>", GetAspNetMasterPageContentContainer()).Replace("<?UMBRACO_TEMPLATE_LOAD_CHILD />", GetAspNetMasterPageContentContainer());
            // Parse the design for aspnet forms
            GetAspNetMasterPageForm(ref masterPageContent);
            masterPageContent = masterPageContent.Replace("</?ASPNET_FORM>", "</form>");
            // Parse the design for aspnet heads
            masterPageContent = masterPageContent.Replace("</ASPNET_HEAD>", String.Format("<head id=\"{0}Head\" runat=\"server\">", Alias.Replace(" ", "")));
            masterPageContent = masterPageContent.Replace("</?ASPNET_HEAD>", "</head>");
            return masterPageContent;
        }



        public void ImportDesign(string design)
        {
            Design = design; 
        }

        public void SaveMasterPageFile(string masterPageContent)
        {
            //this will trigger the helper and store everything
            this.Design = masterPageContent;
        }        
        
        private void GetAspNetMasterPageForm(ref string design)
        {
            Match formElement = Regex.Match(design, GetElementRegExp("?ASPNET_FORM", false), RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            if (formElement != null && formElement.Value != "")
            {
                string formReplace = String.Format("<form id=\"{0}Form\" runat=\"server\">", Alias.Replace(" ", ""));
                if (formElement.Groups.Count == 0)
                {
                    formReplace += "<asp:scriptmanager runat=\"server\"></asp:scriptmanager>";
                }
                design = design.Replace(formElement.Value, formReplace);
            }
        }

        private string GetAspNetMasterPageContentContainer()
        {
            return String.Format(
                "<asp:ContentPlaceHolder ID=\"{0}ContentPlaceHolder\" runat=\"server\"></asp:ContentPlaceHolder>",
                Alias.Replace(" ", ""));
        }

        private void ReplaceElement(ref string design, string elementName, string newElementName, bool checkForQuotes)
        {
            MatchCollection m =
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



        private string GetElementRegExp(string elementName, bool checkForQuotes)
        {
            if (checkForQuotes)
                return String.Format("\"<[^>\\s]*\\b{0}(\\b[^>]*)>\"", elementName);
            else
                return String.Format("<[^>\\s]*\\b{0}(\\b[^>]*)>", elementName);

        }

        [Obsolete("Umbraco automatically ensures that template cache is cleared when saving or deleting")]
        protected virtual void FlushCache()
        {
            //ApplicationContext.Current.ApplicationCache.ClearCacheItem(GetCacheKey(Id));
        }

        public static Template GetTemplate(int id)
        {
            var found = ApplicationContext.Current.Services.FileService.GetTemplate(id);
            return found == null ? null : new Template(found);
        }

        public static Template Import(XmlNode n, IUser u)
        {
            var element = System.Xml.Linq.XElement.Parse(n.OuterXml);
            var templates = ApplicationContext.Current.Services.PackagingService.ImportTemplates(element, u.Id);
            return new Template(templates.First().Id);
        }
        


    }
}
