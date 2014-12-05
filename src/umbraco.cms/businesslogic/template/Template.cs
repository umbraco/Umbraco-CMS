using System;
using System.Linq;
using System.Collections;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using umbraco.DataLayer;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;
using umbraco.cms.businesslogic.cache;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;

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
        //private string _OutputContentType;
        //private string _design;
        //private string _alias;
        //private string _oldAlias;
        private int? _mastertemplate;
        //private bool _hasChildrenInitialized = false;
        //private bool _hasChildren;

        #endregion

        #region Static members

        public static readonly string UmbracoMasterTemplate = SystemDirectories.Umbraco + "/masterpages/default.master";
        private static Hashtable _templateAliases = new Hashtable();
        //private static volatile bool _templateAliasesInitialized = false;
        //private static readonly object TemplateLoaderLocker = new object();
        //private static readonly Guid ObjectType = new Guid(Constants.ObjectTypes.Template);
        //private static readonly char[] NewLineChars = Environment.NewLine.ToCharArray();

        #endregion

		[Obsolete("Use TemplateFilePath instead")]
        public string MasterPageFile
        {
            get { return TemplateFilePath; }
        }

		/// <summary>
		/// Returns the file path for the current template
		/// </summary>
	    public string TemplateFilePath
	    {
		    get
		    {
				switch (TemplateEntity.GetTypeOfRenderingEngine())
				{
					case RenderingEngine.Mvc:
                        return _viewHelper.GetPhysicalFilePath(TemplateEntity);
					case RenderingEngine.WebForms:
                        return _masterPageHelper.GetPhysicalFilePath(TemplateEntity);
					default:
						throw new ArgumentOutOfRangeException();
				}	  
		    }
	    }

        [Obsolete("This is not used at all, do not use this")]
        public static Hashtable TemplateAliases
        {
            get { return _templateAliases; }
            set { _templateAliases = value; }
        }

        #region Constructors

        internal Template(ITemplate template)
            : base(template.Id, true)
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
            SaveEventArgs e = new SaveEventArgs();
            FireBeforeSave(e);

            if (!e.Cancel)
            {
                ApplicationContext.Current.Services.FileService.SaveTemplate(TemplateEntity);
                //base.Save();
                FireAfterSave(e);
            }
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
                        if (Dictionary.DictionaryItem.hasKey(tempText.Substring(1, tempText.Length - 1)))
                        {
                            Dictionary.DictionaryItem di = new Dictionary.DictionaryItem(tempText.Substring(1, tempText.Length - 1));
                            if (di != null)
                                return di.Value(lang.id);
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

            //base.setupNode();

            //IRecordsReader dr = SqlHelper.ExecuteReader("Select alias,design,master from cmsTemplate where nodeId = " + this.Id);
            //bool hasRows = dr.Read();
            //if (hasRows)
            //{
            //    _alias = dr.GetString("alias");
            //    _design = dr.GetString("design");
            //    //set the master template to zero if it's null
            //    _mastertemplate = dr.IsNull("master") ? 0 : dr.GetInt("master");
            //}
            //dr.Close();

            //if (UmbracoConfig.For.UmbracoSettings().Templates.DefaultRenderingEngine == RenderingEngine.Mvc && ViewHelper.ViewExists(this))
            //    _design = ViewHelper.GetFileContents(this);
            //else
            //    _design = MasterPageHelper.GetFileContents(this);

        }
		
        public new string Path
        {
            get
            {
                return TemplateEntity.Path;

                //List<int> path = new List<int>();
                //Template working = this;
                //while (working != null)
                //{
                //    path.Add(working.Id);
                //    try
                //    {
                //        if (working.MasterTemplate != 0)
                //        {
                //            working = new Template(working.MasterTemplate);
                //        }
                //        else
                //        {
                //            working = null;
                //        }
                //    }
                //    catch (ArgumentException)
                //    {
                //        working = null;
                //    }
                //}
                //path.Add(-1);
                //path.Reverse();
                //string sPath = string.Join(",", path.ConvertAll(item => item.ToString()).ToArray());
                //return sPath;
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

                //FlushCache();

                //_design = value.Trim(NewLineChars);

                ////we only switch to MVC View editing if the template has a view file, and MVC editing is enabled
                //if (UmbracoConfig.For.UmbracoSettings().Templates.DefaultRenderingEngine == RenderingEngine.Mvc && !MasterPageHelper.IsMasterPageSyntax(_design))
                //{
                //    MasterPageHelper.RemoveMasterPageFile(this.Alias);
                //    MasterPageHelper.RemoveMasterPageFile(_oldAlias);
                //    _design = ViewHelper.UpdateViewFile(this, _oldAlias);
                //}
                //else if (UmbracoConfig.For.UmbracoSettings().Templates.UseAspNetMasterPages)
                //{
                //    ViewHelper.RemoveViewFile(this.Alias);
                //    ViewHelper.RemoveViewFile(_oldAlias);
                //    _design = MasterPageHelper.UpdateMasterPageFile(this, _oldAlias);
                //}


                //SqlHelper.ExecuteNonQuery("Update cmsTemplate set design = @design where NodeId = @id",
                //        SqlHelper.CreateParameter("@design", _design),
                //        SqlHelper.CreateParameter("@id", Id));
            }
        }

        public XmlNode ToXml(XmlDocument doc)
        {
            var serializer = new EntityXmlSerializer();
            var serialized = serializer.Serialize(TemplateEntity);
            return serialized.GetXmlNode(doc);

            //XmlNode template = doc.CreateElement("Template");
            //template.AppendChild(xmlHelper.addTextNode(doc, "Name", base.Text));
            //template.AppendChild(xmlHelper.addTextNode(doc, "Alias", this.Alias));

            //if (this.MasterTemplate != 0)
            //{
            //    template.AppendChild(xmlHelper.addTextNode(doc, "Master", new Template(this.MasterTemplate).Alias));
            //}

            //template.AppendChild(xmlHelper.addCDataNode(doc, "Design", this.Design));

            //return template;
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
                }
            }

            RemoveFromDocumentTypes();

            // remove from documents
            Document.RemoveTemplateFromDocument(this.Id);
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

        public static Template MakeNew(string Name, User u, Template master)
        {
            return MakeNew(Name, u, master, null);
        }
        
        private static Template MakeNew(string name, User u, string design)
        {
            return MakeNew(name, u, null, design);
        }

        public static Template MakeNew(string name, User u)
        {
            return MakeNew(name, u, design: null);
        }

        private static Template MakeNew(string name, User u, Template master, string design)
        {
            var foundMaster = ApplicationContext.Current.Services.FileService.GetTemplate(master.Id);
            var template = ApplicationContext.Current.Services.FileService.CreateTemplateWithIdentity(name, design, foundMaster, u.Id);

            var legacyTemplate = new Template(template);

            //// CMSNode MakeNew(int parentId, Guid objectType, int userId, int level, string text, Guid uniqueID)
            //var node = MakeNew(-1, ObjectType, u.Id, 1, name, Guid.NewGuid());

            ////ensure unique alias 
            //name = name.ToCleanString(CleanStringType.UnderscoreAlias);
            //if (GetByAlias(name) != null)
            //    name = EnsureUniqueAlias(name, 1);
            ////name = name.Replace("/", ".").Replace("\\", ""); //why? ToSafeAlias() already removes those chars

            //if (name.Length > 100)
            //    name = name.Substring(0, 95); // + "..."; // no, these are invalid alias chars
          
            //SqlHelper.ExecuteNonQuery("INSERT INTO cmsTemplate (NodeId, Alias, design, master) VALUES (@nodeId, @alias, @design, @master)",
            //                          SqlHelper.CreateParameter("@nodeId", node.Id),
            //                          SqlHelper.CreateParameter("@alias", name),
            //                          SqlHelper.CreateParameter("@design", ' '),
            //                          SqlHelper.CreateParameter("@master", DBNull.Value));

            //var template = new Template(node.Id);
            //if (master != null)
            //    template.MasterTemplate = master.Id;

            //switch (DetermineRenderingEngine(template, design))
            //{
            //    case RenderingEngine.Mvc:
            //        ViewHelper.CreateViewFile(template);
            //        break;
            //    case RenderingEngine.WebForms:
            //        MasterPageHelper.CreateMasterPage(template);
            //        break;
            //}

            ////if a design is supplied ensure it is updated.
            //if (design.IsNullOrWhiteSpace() == false)
            //{
            //    template.ImportDesign(design);
            //}

            var e = new NewEventArgs();
            legacyTemplate.OnNew(e);

            return legacyTemplate;
        }

        //private static string EnsureUniqueAlias(string alias, int attempts)
        //{
        //    if (GetByAlias(alias + attempts.ToString()) == null)
        //        return alias + attempts.ToString();
        //    else
        //    {
        //        attempts++;
        //        return EnsureUniqueAlias(alias, attempts);
        //    }
        //}

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

        //private static void InitTemplateAliases()
        //{
        //    if (!_templateAliasesInitialized)
        //    {
        //        lock (TemplateLoaderLocker)
        //        {
        //            //double check
        //            if (!_templateAliasesInitialized)
        //            {
        //                _templateAliases.Clear();
        //                foreach (Template t in GetAllAsList())
        //                    TemplateAliases.Add(t.Alias.ToLower(), t.Id);

        //                _templateAliasesInitialized = true;
        //            }

        //        }
        //    }
        //}

        public override void delete()
        {
            // don't allow template deletion if it has child templates
            if (this.HasChildren)
            {                
                var ex = new InvalidOperationException("Can't delete a master template. Remove any bindings from child templates first.");
				LogHelper.Error<Template>("Can't delete a master template. Remove any bindings from child templates first.", ex);
	            throw ex;
            }

            //// NH: Changed this; if you delete a template we'll remove all references instead of 
            //// throwing an exception
            //if (DocumentType.GetAllAsList().Where(x => x.allowedTemplates.Select(t => t.Id).Contains(this.Id)).Count() > 0)
            //    RemoveAllReferences();

            DeleteEventArgs e = new DeleteEventArgs();
            FireBeforeDelete(e);

            if (!e.Cancel)
            {
                ////re-set the template aliases
                //_templateAliasesInitialized = false;
                //InitTemplateAliases();

                ////delete the template
                //SqlHelper.ExecuteNonQuery("delete from cmsTemplate where NodeId =" + this.Id);

                //base.delete();

                //// remove masterpages
                //if (System.IO.File.Exists(MasterPageFile))
                //    System.IO.File.Delete(MasterPageFile);

                //if (System.IO.File.Exists(Umbraco.Core.IO.IOHelper.MapPath(ViewHelper.ViewPath(this.Alias))))
                //    System.IO.File.Delete(Umbraco.Core.IO.IOHelper.MapPath(ViewHelper.ViewPath(this.Alias)));

                ApplicationContext.Current.Services.FileService.DeleteTemplate(TemplateEntity.Alias);

                FireAfterDelete(e);
            }
        }

        [Obsolete("This method, doesnt actually do anything, as the file is created when the design is set", false)]
        public void _SaveAsMasterPage()
        {
            //SaveMasterPageFile(ConvertToMasterPageSyntax(Design));
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

            //return ApplicationContext.Current.ApplicationCache.GetCacheItem(
            //    GetCacheKey(id),
            //    TimeSpan.FromMinutes(30),
            //    () =>
            //        {
            //            try
            //            {
            //                return new Template(id);
            //            }
            //            catch
            //            {
            //                return null;
            //            }
            //        });
        }

        //private static string GetCacheKey(int id)
        //{
        //    return CacheKeys.TemplateBusinessLogicCacheKey + id;
        //}
		
        public static Template Import(XmlNode n, User u)
        {
            var element = System.Xml.Linq.XElement.Parse(n.OuterXml);
            var templates = ApplicationContext.Current.Services.PackagingService.ImportTemplates(element, u.Id);
            return new Template(templates.First().Id);
        }
        

        #region Events
        //EVENTS
        /// <summary>
        /// The save event handler
        /// </summary>
        public delegate void SaveEventHandler(Template sender, SaveEventArgs e);
        /// <summary>
        /// The new event handler
        /// </summary>
        public delegate void NewEventHandler(Template sender, NewEventArgs e);
        /// <summary>
        /// The delete event handler
        /// </summary>
        public delegate void DeleteEventHandler(Template sender, DeleteEventArgs e);


        /// <summary>
        /// Occurs when [before save].
        /// </summary>
        public new static event SaveEventHandler BeforeSave;
        /// <summary>
        /// Raises the <see cref="E:BeforeSave"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected override void FireBeforeSave(SaveEventArgs e)
        {
            if (BeforeSave != null)
                BeforeSave(this, e);
        }

        /// <summary>
        /// Occurs when [after save].
        /// </summary>
        public new static event SaveEventHandler AfterSave;
        /// <summary>
        /// Raises the <see cref="E:AfterSave"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected override void FireAfterSave(SaveEventArgs e)
        {
            if (AfterSave != null)
                AfterSave(this, e);
        }

        /// <summary>
        /// Occurs when [new].
        /// </summary>
        public static event NewEventHandler New;
        /// <summary>
        /// Raises the <see cref="E:New"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnNew(NewEventArgs e)
        {
            if (New != null)
                New(this, e);
        }

        /// <summary>
        /// Occurs when [before delete].
        /// </summary>
        public new static event DeleteEventHandler BeforeDelete;
        /// <summary>
        /// Raises the <see cref="E:BeforeDelete"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected override void FireBeforeDelete(DeleteEventArgs e)
        {
            if (BeforeDelete != null)
                BeforeDelete(this, e);
        }

        /// <summary>
        /// Occurs when [after delete].
        /// </summary>
        public new static event DeleteEventHandler AfterDelete;
        /// <summary>
        /// Raises the <see cref="E:AfterDelete"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected override void FireAfterDelete(DeleteEventArgs e)
        {
            if (AfterDelete != null)
                AfterDelete(this, e);
        }
        #endregion

    }
}
