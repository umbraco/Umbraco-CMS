using System;
using System.Collections;
using System.Xml;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using umbraco.DataLayer;
using System.Collections.Generic;
using System.Threading;
using Umbraco.Core;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;

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

        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentTypeService.GetContentType()", false)]
        public new static DocumentType GetByAlias(string Alias)
        {
            try
            {
                var contentType = ApplicationContext.Current.Services.ContentTypeService.Get(Alias);
                return new DocumentType(contentType.Id);
            }
            catch
            {
                return null;
            }
        }

        [Obsolete("Obsolete, Use Umbraco.Core.Models.ContentType and Umbraco.Core.Services.ContentTypeService.Save()", false)]
        public static DocumentType MakeNew(IUser u, string Text)
        {
            var contentType = new Umbraco.Core.Models.ContentType(-1) { Name = Text, Alias = Text, CreatorId = u.Id, Thumbnail = "icon-folder", Icon = "icon-folder" };
            ApplicationContext.Current.Services.ContentTypeService.Save(contentType, u.Id);
            var newDt = new DocumentType(contentType);


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
            var contentTypes = ApplicationContext.Current.Services.ContentTypeService.GetAll();
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
            // check that no document types uses me as a master
            if (GetAllAsList().Any(dt => dt.MasterContentTypes.Contains(this.Id)))
            {
                throw new ArgumentException("Can't delete a Document Type used as a Master Content Type. Please remove all references first!");
            }
                
            ApplicationContext.Current.Services.ContentTypeService.Delete(ContentType);

            clearTemplates();

        }

        public void clearTemplates()
        {
            ContentType.AllowedTemplates = new List<ITemplate>();
            _templateIds.Clear();
        }

        public XmlElement ToXml(XmlDocument xd)
        {
            var exporter = new EntityXmlSerializer();
            var xml = exporter.Serialize(
                ApplicationContext.Current.Services.DataTypeService,
                ApplicationContext.Current.Services.ContentTypeService,
                ContentType);

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
            var current = Thread.CurrentPrincipal != null ? Thread.CurrentPrincipal.Identity as UmbracoBackOfficeIdentity : null;
            var userId = current == null ? Attempt<int>.Fail() : current.Id.TryConvertTo<int>();                
            ApplicationContext.Current.Services.ContentTypeService.Save(ContentType, userId.Success ? userId.Result : 0);

            base.Save();
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
            var contentType = ApplicationContext.Current.Services.ContentTypeService.Get(Id);
            
            // If it's null, it's probably a folder
            if (contentType != null)
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

    }
}