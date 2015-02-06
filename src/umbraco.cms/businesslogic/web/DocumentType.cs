using System;
using System.Collections;
using System.Text;
using System.Xml;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using umbraco.BusinessLogic;
using Umbraco.Core.Services;
using umbraco.DataLayer;
using System.Collections.Generic;
using Umbraco.Core;

namespace umbraco.cms.businesslogic.web
{
    /// <summary>
    /// Summary description for DocumentType.
    /// </summary>
    [Obsolete("Obsolete, Use Umbraco.Core.Models.ContentType", false)]
    public class DocumentType : ContentType
    {
        #region Constructors

        public DocumentType(int id) : base(id) { }

        public DocumentType(Guid id) : base(id) { }

        public DocumentType(int id, bool noSetup) : base(id, noSetup) { }

        internal DocumentType(IContentType contentType)
            : base(contentType)
        {
            SetupNode(contentType);
        }

        #endregion

        #region Constants and Static members

        public static Guid _objectType = new Guid(Constants.ObjectTypes.DocumentType);

        new internal const string m_SQLOptimizedGetAll = @"
            SELECT id, createDate, trashed, parentId, nodeObjectType, nodeUser, level, path, sortOrder, uniqueID, text,
                allowAtRoot, isContainer, Alias,icon,thumbnail,description,
                templateNodeId, IsDefault
            FROM umbracoNode 
            INNER JOIN cmsContentType ON umbracoNode.id = cmsContentType.nodeId
            LEFT OUTER JOIN cmsDocumentType ON cmsContentType.nodeId = cmsDocumentType.contentTypeNodeId
            WHERE nodeObjectType = @nodeObjectType";

        #endregion

        #region Private Members

        private ArrayList _templateIds = new ArrayList();
        private int _defaultTemplate;
        private bool _hasChildrenInitialized = false;
        private bool _hasChildren;

        private IContentType ContentType
        {
            get { return base.ContentTypeItem as IContentType; }
            set { base.ContentTypeItem = value; }
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Generates the complete (simplified) XML DTD 
        /// </summary>
        /// <returns>The DTD as a string</returns>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentTypeService.GetDtd()", false)]
        public static string GenerateDtd()
        {
            StringBuilder dtd = new StringBuilder();
            // Renamed 'umbraco' to 'root' since the top level of the DOCTYPE should specify the name of the root node for it to be valid;
            // there's no mention of 'umbraco' anywhere in the schema that this DOCTYPE governs
            // (Alex N 20100212)
            dtd.AppendLine("<!DOCTYPE root [ ");

            dtd.AppendLine(GenerateXmlDocumentType());
            dtd.AppendLine("]>");

            return dtd.ToString();
        }

        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentTypeService.GetContentTypesDtd()", false)]
        public static string GenerateXmlDocumentType()
        {
            StringBuilder dtd = new StringBuilder();
            if (UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema)
            {
                dtd.AppendLine("<!ELEMENT node ANY> <!ATTLIST node id ID #REQUIRED>  <!ELEMENT data ANY>");
            }
            else
            {
                // TEMPORARY: Added Try-Catch to this call since trying to generate a DTD against a corrupt db
                // or a broken connection string is not handled yet
                // (Alex N 20100212)
                try
                {
                    StringBuilder strictSchemaBuilder = new StringBuilder();

                    List<DocumentType> dts = GetAllAsList();
                    foreach (DocumentType dt in dts)
                    {
                        string safeAlias = helpers.Casing.SafeAlias(dt.Alias);
                        if (safeAlias != null)
                        {
                            strictSchemaBuilder.AppendLine(String.Format("<!ELEMENT {0} ANY>", safeAlias));
                            strictSchemaBuilder.AppendLine(String.Format("<!ATTLIST {0} id ID #REQUIRED>", safeAlias));
                        }
                    }

                    // Only commit the strong schema to the container if we didn't generate an error building it
                    dtd.Append(strictSchemaBuilder);
                }
                catch (Exception exception)
                {
                    LogHelper.Error<DocumentType>("Exception while trying to build DTD for Xml schema; is Umbraco installed correctly and the connection string configured?", exception);
                }

            }
            return dtd.ToString();

        }

        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentTypeService.GetContentType()", false)]
        public new static DocumentType GetByAlias(string Alias)
        {
            try
            {
                var contentType = ApplicationContext.Current.Services.ContentTypeService.GetContentType(Alias);
                return new DocumentType(contentType.Id);
            }
            catch
            {
                return null;
            }
        }

        [Obsolete("Obsolete, Use Umbraco.Core.Models.ContentType and Umbraco.Core.Services.ContentTypeService.Save()", false)]
        public static DocumentType MakeNew(User u, string Text)
        {
            var contentType = new Umbraco.Core.Models.ContentType(-1) { Name = Text, Alias = Text, CreatorId = u.Id, Thumbnail = "icon-folder", Icon = "icon-folder" };
            ApplicationContext.Current.Services.ContentTypeService.Save(contentType, u.Id);
            var newDt = new DocumentType(contentType);

            //event
            NewEventArgs e = new NewEventArgs();
            newDt.OnNew(e);

            return newDt;
        }

        [Obsolete("Use GetAllAsList() method call instead", true)]
        public new static DocumentType[] GetAll
        {
            get
            {
                return GetAllAsList().ToArray();
            }
        }

        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentTypeService.GetAllContentTypes()", false)]
        public static List<DocumentType> GetAllAsList()
        {
            var contentTypes = ApplicationContext.Current.Services.ContentTypeService.GetAllContentTypes();
            var documentTypes = contentTypes.Select(x => new DocumentType(x));

            return documentTypes.OrderBy(x => x.Text).ToList();
        }

        #endregion

        #region Public Properties
        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentTypeService.HasChildren()", false)]
        public override bool HasChildren
        {
            get
            {
                if (_hasChildrenInitialized == false)
                {
                    HasChildren = ApplicationContext.Current.Services.ContentTypeService.HasChildren(Id);
                }
                return _hasChildren;
            }
            set
            {
                _hasChildrenInitialized = true;
                _hasChildren = value;
            }
        }

        [Obsolete("Obsolete, Use SetDefaultTemplate() on Umbraco.Core.Models.ContentType", false)]
        public int DefaultTemplate
        {
            get { return _defaultTemplate; }
            set
            {
                RemoveDefaultTemplate();
                _defaultTemplate = value;

                if (_defaultTemplate != 0)
                {
                    var template = ApplicationContext.Current.Services.FileService.GetTemplate(_defaultTemplate);
                    ContentType.SetDefaultTemplate(template);
                }
            }
        }

        /// <summary>
        /// Gets/sets the allowed templates for this document type.
        /// </summary>
        [Obsolete("Obsolete, Use AllowedTemplates property on Umbraco.Core.Models.ContentType", false)]
        public template.Template[] allowedTemplates
        {
            get
            {
                if (HasTemplate())
                {
                    template.Template[] retval = new template.Template[_templateIds.Count];
                    for (int i = 0; i < _templateIds.Count; i++)
                    {
                        retval[i] = template.Template.GetTemplate((int)_templateIds[i]);
                    }
                    return retval;
                }
                return new template.Template[0];
            }
            set
            {
                clearTemplates();
                var templates = new List<ITemplate>();
                foreach (template.Template t in value)
                {
                    var template = ApplicationContext.Current.Services.FileService.GetTemplate(t.Id);
                    templates.Add(template);

                    _templateIds.Add(t.Id);
                }
                ContentType.AllowedTemplates = templates;
            }
        }

        public new string Path
        {
            get
            {
                List<int> path = new List<int>();
                DocumentType working = this;
                while (working != null)
                {
                    path.Add(working.Id);
                    try
                    {
                        if (working.MasterContentType != 0)
                        {
                            working = new DocumentType(working.MasterContentType);
                        }
                        else
                        {
                            working = null;
                        }
                    }
                    catch (ArgumentException)
                    {
                        working = null;
                    }
                }
                path.Add(-1);
                path.Reverse();
                string sPath = string.Join(",", path.ConvertAll(item => item.ToString()).ToArray());
                return sPath;
            }
            set
            {
                base.Path = value;
            }
        }
        #endregion

        #region Public Methods

        #region Regenerate Xml Structures

        /// <summary>
        /// This will return all PUBLISHED content Ids that are of this content type or any descendant types as well.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This will also work if we have multiple child content types (if one day we support that)
        /// </remarks>
        internal override IEnumerable<int> GetContentIdsForContentType()
        {
            var ids = new List<int>();

            var currentContentTypeIds = new[] { this.Id };
            while (currentContentTypeIds.Any())
            {
                var childContentTypeIds = new List<int>();

                //loop through each content type id
                foreach (var currentContentTypeId in currentContentTypeIds)
                {
                    //get all the content item ids of the current content type
                    using (var dr = SqlHelper.ExecuteReader(@"SELECT DISTINCT cmsDocument.nodeId FROM cmsDocument
                                                        INNER JOIN cmsContent ON cmsContent.nodeId = cmsDocument.nodeId
                                                        INNER JOIN cmsContentType ON cmsContent.contentType = cmsContentType.nodeId
                                                        WHERE cmsContentType.nodeId = @contentTypeId AND cmsDocument.published = 1",
                                                            SqlHelper.CreateParameter("@contentTypeId", currentContentTypeId)))
                    {
                        while (dr.Read())
                        {
                            ids.Add(dr.GetInt("nodeId"));
                        }
                        dr.Close();
                    }

                    //lookup the child content types if there are any and add the ids to the content type ids array
                    using (var reader = SqlHelper.ExecuteReader("SELECT childContentTypeId FROM cmsContentType2ContentType WHERE parentContentTypeId=@contentTypeId",
                                                                SqlHelper.CreateParameter("@contentTypeId", currentContentTypeId)))
                    {
                        while (reader.Read())
                        {
                            childContentTypeIds.Add(reader.GetInt("childContentTypeId"));
                        }
                    }
      
                }

                currentContentTypeIds = childContentTypeIds.ToArray();       
                
            }
            return ids;
        } 

        /// <summary>
        /// Rebuilds the xml structure for the content item by id
        /// </summary>
        /// <param name="contentId"></param>
        /// <remarks>
        /// This is not thread safe
        /// </remarks>
        internal override void RebuildXmlStructureForContentItem(int contentId)
        {
            var xd = new XmlDocument();
            try
            {
                //create the document in optimized mode! 
                // (not sure why we wouldn't always do that ?!)

                new Document(true, contentId).XmlGenerate(xd);

                //The benchmark results that I found based contructing the Document object with 'true' for optimized
                //mode, vs using the normal ctor. Clearly optimized mode is better!
                /*
                 * The average page rendering time (after 10 iterations) for submitting /umbraco/dialogs/republish?xml=true when using 
                 * optimized mode is
                 * 
                 * 0.060400555555556
                 * 
                 * The average page rendering time (after 10 iterations) for submitting /umbraco/dialogs/republish?xml=true when not
                 * using optimized mode is
                 * 
                 * 0.107037777777778
                 *                      
                 * This means that by simply changing this to use optimized mode, it is a 45% improvement!
                 * 
                 */
            }
            catch (Exception ee)
            {
                LogHelper.Error<DocumentType>("Error generating xml", ee);
            }
        }

        /// <summary>
        /// Clears all xml structures in the cmsContentXml table for the current content type and any of it's descendant types
        /// </summary>
        /// <remarks>
        /// This is not thread safe.
        /// This will also work if we have multiple child content types (if one day we support that)
        /// </remarks>
        internal override void ClearXmlStructuresForContent()
        {
            var currentContentTypeIds = new[] {this.Id};
            while (currentContentTypeIds.Any())
            {
                var childContentTypeIds = new List<int>();

                //loop through each content type id
                foreach (var currentContentTypeId in currentContentTypeIds)
                {
                    //Remove all items from the cmsContentXml table that are of this current content type
                    SqlHelper.ExecuteNonQuery(@"DELETE FROM cmsContentXml WHERE nodeId IN
                                        (SELECT DISTINCT cmsContent.nodeId FROM cmsContent 
                                            INNER JOIN cmsContentType ON cmsContent.contentType = cmsContentType.nodeId 
                                            WHERE cmsContentType.nodeId = @contentTypeId)",
                                              SqlHelper.CreateParameter("@contentTypeId", currentContentTypeId));

                    //lookup the child content types if there are any and add the ids to the content type ids array
                    using (var reader = SqlHelper.ExecuteReader("SELECT childContentTypeId FROM cmsContentType2ContentType WHERE parentContentTypeId=@contentTypeId",
                                                                SqlHelper.CreateParameter("@contentTypeId", currentContentTypeId)))
                    {                        
                        while (reader.Read())
                        {
                            childContentTypeIds.Add(reader.GetInt("childContentTypeId"));
                        }                        
                    }      

                }

                currentContentTypeIds = childContentTypeIds.ToArray();                          
            }
        } 

        #endregion

        [Obsolete("Obsolete, Use RemoveTemplate() on Umbraco.Core.Models.ContentType", false)]
        public void RemoveTemplate(int templateId)
        {
            // remove if default template
            if (this.DefaultTemplate == templateId)
            {
                RemoveDefaultTemplate();
            }

            // remove from list of document type templates
            if (_templateIds.Contains(templateId))
            {
                var template = ContentType.AllowedTemplates.FirstOrDefault(x => x.Id == templateId);
                if (template != null)
                    ContentType.RemoveTemplate(template);

                _templateIds.Remove(templateId);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ArgumentException">Throws an exception if trying to delete a document type that is assigned as a master document type</exception>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentTypeService.Delete()", false)]
        public override void delete()
        {
            DeleteEventArgs e = new DeleteEventArgs();
            FireBeforeDelete(e);

            if (e.Cancel == false)
            {
                // check that no document types uses me as a master
                if (GetAllAsList().Any(dt => dt.MasterContentTypes.Contains(this.Id)))
                {
                    throw new ArgumentException("Can't delete a Document Type used as a Master Content Type. Please remove all references first!");
                }
                
                ApplicationContext.Current.Services.ContentTypeService.Delete(ContentType);

                clearTemplates();

                FireAfterDelete(e);
            }
        }

        public void clearTemplates()
        {
            ContentType.AllowedTemplates = new List<ITemplate>();
            _templateIds.Clear();
        }

        public XmlElement ToXml(XmlDocument xd)
        {
            var exporter = new EntityXmlSerializer();
            var xml = exporter.Serialize(ApplicationContext.Current.Services.DataTypeService, ContentType);

            //convert the Linq to Xml structure to the old .net xml structure
            var xNode = xml.GetXmlNode();
            var doc = (XmlElement)xd.ImportNode(xNode, true);
            return doc;
        }

        [Obsolete("Obsolete, Use SetDefaultTemplate(null) on Umbraco.Core.Models.ContentType", false)]
        public void RemoveDefaultTemplate()
        {
            _defaultTemplate = 0;
            ContentType.SetDefaultTemplate(null);
        }

        public bool HasTemplate()
        {
            return (_templateIds.Count > 0);
        }

        /// <summary>
        /// Used to persist object changes to the database. In Version3.0 it's just a stub for future compatibility
        /// </summary>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentTypeService.Save()", false)]
        public override void Save()
        {
            var e = new SaveEventArgs();
            FireBeforeSave(e);

            if (!e.Cancel)
            {
                var current = User.GetCurrent();
                int userId = current == null ? 0 : current.Id;
                ApplicationContext.Current.Services.ContentTypeService.Save(ContentType, userId);

                base.Save();
                FireAfterSave(e);
            }
        }

        #endregion

        #region Protected Methods

        [Obsolete("Depreated, No longer needed nor used")]
        protected void PopulateDocumentTypeNodeFromReader(IRecordsReader dr)
        {
            if (!dr.IsNull("templateNodeId"))
            {
                _templateIds.Add(dr.GetInt("templateNodeId"));
                if (!dr.IsNull("IsDefault"))
                {
                    if (dr.GetBoolean("IsDefault"))
                    {
                        _defaultTemplate = dr.GetInt("templateNodeId");
                    }
                }
            }
        }

        protected override void setupNode()
        {
            var contentType = ApplicationContext.Current.Services.ContentTypeService.GetContentType(Id);
            SetupNode(contentType);
        }

        #endregion

        #region Private Methods
        private void SetupNode(IContentType contentType)
        {
            ContentType = contentType;
            foreach (var template in ContentType.AllowedTemplates.Where(t => t != null))
            {
                _templateIds.Add(template.Id);
            }

            if (ContentType.DefaultTemplate != null)
                _defaultTemplate = ContentType.DefaultTemplate.Id;

            base.PopulateContentTypeFromContentTypeBase(ContentType);
            base.PopulateCMSNodeFromUmbracoEntity(ContentType, _objectType);
        }
        #endregion

        #region Events
        /// <summary>
        /// The save event handler
        /// </summary>
        public delegate void SaveEventHandler(DocumentType sender, SaveEventArgs e);
        /// <summary>
        /// The New event handler
        /// </summary>
        public delegate void NewEventHandler(DocumentType sender, NewEventArgs e);
        /// <summary>
        /// The delete event handler
        /// </summary>
        public delegate void DeleteEventHandler(DocumentType sender, DeleteEventArgs e);

        /// <summary>
        /// Occurs when [before save].
        /// </summary>
        public static event SaveEventHandler BeforeSave;
        /// <summary>
        /// Raises the <see cref="E:BeforeSave"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void FireBeforeSave(SaveEventArgs e)
        {
            if (BeforeSave != null)
                BeforeSave(this, e);
        }

        /// <summary>
        /// Occurs when [after save].
        /// </summary>
        public static event SaveEventHandler AfterSave;
        /// <summary>
        /// Raises the <see cref="E:AfterSave"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void FireAfterSave(SaveEventArgs e)
        {
            if (AfterSave != null)
            {
                var updated = this.ContentType == null
                                  ? new DocumentType(this.Id)
                                  : new DocumentType(this.ContentType);
                AfterSave(updated, e);
            }
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
        public static event DeleteEventHandler BeforeDelete;
        /// <summary>
        /// Raises the <see cref="E:BeforeDelete"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void FireBeforeDelete(DeleteEventArgs e)
        {
            if (BeforeDelete != null)
                BeforeDelete(this, e);
        }

        /// <summary>
        /// Occurs when [after delete].
        /// </summary>
        public static event DeleteEventHandler AfterDelete;
        /// <summary>
        /// Raises the <see cref="E:AfterDelete"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void FireAfterDelete(DeleteEventArgs e)
        {
            if (AfterDelete != null)
                AfterDelete(this, e);
        }
        #endregion
    }
}