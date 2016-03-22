using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using umbraco.DataLayer;
using Umbraco.Core.Models.Membership;

namespace umbraco.cms.businesslogic.web
{
    /// <summary>
    /// Document represents a webpage,
    /// type (umbraco.cms.businesslogic.web.DocumentType)
    /// 
    /// Pubished Documents are exposed to the runtime/the public website in a cached xml document.
    /// </summary>
    [Obsolete("Obsolete, Use Umbraco.Core.Models.Content", false)]
    public class Document : Content
    {
        #region Constructors

        /// <summary>
        /// Constructs a new document
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <param name="noSetup">true if the data shouldn't loaded from the db</param>
        public Document(Guid id, bool noSetup) : base(id, noSetup) { }

        /// <summary>
        /// Initializes a new instance of the Document class.
        /// You can set an optional flag noSetup, used for optimizing for loading nodes in the tree, 
        /// therefor only data needed by the tree is initialized.
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <param name="noSetup">true if the data shouldn't loaded from the db</param>
        public Document(int id, bool noSetup) : base(id, noSetup) { }

        /// <summary>
        /// Initializes a new instance of the Document class to a specific version, used for rolling back data from a previous version
        /// of the document.
        /// </summary>
        /// <param name="id">The id of the document</param>
        /// <param name="Version">The version of the document</param>
        public Document(int id, Guid Version)
            : base(id, Version)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Document class.
        /// </summary>
        /// <param name="id">The id of the document</param>
        public Document(int id) : base(id) { }

        /// <summary>
        /// Initialize the document
        /// </summary>
        /// <param name="id">The id of the document</param>
        public Document(Guid id) : base(id) { }      

        /// <summary>
        /// Internal initialization of a legacy Document object using the new IUmbracoEntity object
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="noSetup"></param>
        internal Document(IUmbracoEntity entity, bool noSetup = true) : base(entity)
        {
            if(noSetup == false)
                setupNode();
        }

        /// <summary>
        /// Internal initialization of a legacy Document object using the new IContent object
        /// </summary>
        /// <param name="content"></param>
        internal Document(IContent content) : base(content)
        {
            SetupNode(content);
        }

        #endregion

        #region Constants and Static members
        
        private const string SqlOptimizedForPreview = @"
                select umbracoNode.id, umbracoNode.parentId, umbracoNode.level, umbracoNode.sortOrder, cmsDocument.versionId, cmsPreviewXml.xml, cmsDocument.published
                from cmsDocument
                inner join umbracoNode on umbracoNode.id = cmsDocument.nodeId
                inner join cmsPreviewXml on cmsPreviewXml.nodeId = cmsDocument.nodeId and cmsPreviewXml.versionId = cmsDocument.versionId
                where newest = 1 and trashed = 0 and path like '{0}'
                order by level,sortOrder
 ";

        public static Guid _objectType = new Guid(Constants.ObjectTypes.Document);

        #endregion

        #region Private properties

        private DateTime _updated;
        private DateTime _release;
        private DateTime _expire;
        private int _template;
        private bool _published;
        private XmlNode _xml;
        private IUser _creator;
        private IUser _writer;
        private int? _writerId;
        private readonly bool _optimizedMode;
        protected internal IContent ContentEntity;

        /// <summary>
        /// This is used to cache the child documents of Document when the children property
        /// is accessed or enumerated over, this will save alot of database calls.
        /// </summary>
        private IEnumerable<Document> _children = null;

        // special for tree performance
        private int _userId = -1;

        #endregion

        #region Static Methods
        

        /// <summary>
        /// Check if a node is a document
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentService.GetById()", false)]
        public static bool IsDocument(int nodeId)
        {
            bool isDoc = false;

            var content = ApplicationContext.Current.Services.ContentService.GetById(nodeId);
            isDoc = content != null;

            return isDoc;
        }

        
        /// <summary>
        /// Used to get the firstlevel/root documents of the hierachy
        /// </summary>
        /// <returns>Root documents</returns>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentService.GetRootContent()", false)]
        public static Document[] GetRootDocuments()
        {
            var content = ApplicationContext.Current.Services.ContentService.GetRootContent().OrderBy(x => x.SortOrder);
            return content.Select(c => new Document(c)).ToArray();
        }
        

        public static void RemoveTemplateFromDocument(int templateId)
        {
            ApplicationContext.Current.DatabaseContext.Database.Execute(
                "update cmsDocument set templateId = NULL where templateId = @TemplateId", new {TemplateId = templateId});
            //We need to clear cache for Documents since this is touching the database directly
            ApplicationContext.Current.ApplicationCache.IsolatedRuntimeCache.ClearCache<IContent>();
        }

        /// <summary>
        /// Performance tuned method for use in the tree
        /// </summary>
        /// <param name="NodeId">The parentdocuments id</param>
        /// <returns></returns>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentService.GetChildren()", false)]
        public static Document[] GetChildrenForTree(int NodeId)
        {
            var children = ApplicationContext.Current.Services.ContentService.GetChildren(NodeId);
            var list = children.Select(x => new Document(x));
            return list.ToArray();
        }
        

        #endregion

        #region Public Properties
        
        public override int sortOrder
        {
            get
            {
                return ContentEntity == null ? base.sortOrder : ContentEntity.SortOrder;
            }
            set
            {
                if (ContentEntity == null)
                {
                    base.sortOrder = value;
                }
                else
                {
                    ContentEntity.SortOrder = value;
                }
            }
        }

        public override int Level
        {
            get
            {
                return ContentEntity == null ? base.Level : ContentEntity.Level;
            }
            set
            {
                if (ContentEntity == null)
                {
                    base.Level = value;
                }
                else
                {
                    ContentEntity.Level = value;
                }
            }
        }

        public override int ParentId
        {
            get { return ContentEntity == null ? base.ParentId : ContentEntity.ParentId; }
        }

        public override string Path
        {
            get
            {
                return ContentEntity == null ? base.Path : ContentEntity.Path;
            }
            set
            {
                if (ContentEntity == null)
                {
                    base.Path = value;
                }
                else
                {
                    ContentEntity.Path = value;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the document was constructed for the optimized mode
        /// </summary>
        /// <value><c>true</c> if the document is working in the optimized mode; otherwise, <c>false</c>.</value>
        public bool OptimizedMode
        {
            get
            {
                return this._optimizedMode;
            }
        }

        /// <summary>
        /// The id of the user whom created the document
        /// </summary>
        public int UserId
        {
            get
            {
                if (_userId == -1)
                    _userId = User.Id;

                return _userId;
            }
        }

        /// <summary>
        /// Gets the user who created the document.
        /// </summary>
        /// <value>The creator.</value>
        public IUser Creator
        {
            get
            {
                if (_creator == null)
                {
                    _creator = User;
                }
                return _creator;
            }
        }

        /// <summary>
        /// Gets the writer.
        /// </summary>
        /// <value>The writer.</value>
        public IUser Writer
        {
            get
            {
                if (_writer == null)
                {
                    if (!_writerId.HasValue)
                    {
                        throw new NullReferenceException("Writer ID has not been specified for this document");
                    }
                    _writer = ApplicationContext.Current.Services.UserService.GetUserById(_writerId.Value);
                }
                return _writer;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the document is published.
        /// </summary>
		/// <remarks>A document can be published yet not visible, because of one or more of its
		/// parents being unpublished. Use <c>PathPublished</c> to get a value indicating whether
		/// the node and all its parents are published, and therefore whether the node is visible.</remarks>
        [Obsolete("Obsolete, Use Published property on Umbraco.Core.Models.Content", false)]
        public bool Published
        {
            get { return _published; }

            set
            {
                _published = value;
                if (_published)
                {
                    ContentEntity.ChangePublishedState(PublishedState.Published);
                }
                else
                {
                    ContentEntity.ChangePublishedState(PublishedState.Unpublished);
                    
                }
            }
        }

		/// <summary>
		/// Gets a value indicating whether the document and all its parents are published.
		/// </summary>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentService.IsPublishable()", false)]
        public bool PathPublished
		{
			get
			{
				return ApplicationContext.Current.Services.ContentService.IsPublishable(ContentEntity);
			}
		}

        [Obsolete("Obsolete, Use Name property on Umbraco.Core.Models.Content", false)]
        public override string Text
        {
            get
            {
                return ContentEntity.Name;
            }
            set
            {
                value = value.Trim();
                ContentEntity.Name = value;
            }
        }

        /// <summary>
        /// The date of the last update of the document
        /// </summary>
        [Obsolete("Obsolete, Use UpdateDate property on Umbraco.Core.Models.Content", false)]
        public DateTime UpdateDate
        {
            get { return _updated; }
            set
            {
                _updated = value;
                ContentEntity.UpdateDate = value;
                /*SqlHelper.ExecuteNonQuery("update cmsDocument set updateDate = @value where versionId = @versionId",
                                          SqlHelper.CreateParameter("@value", value),
                                          SqlHelper.CreateParameter("@versionId", Version));*/
            }
        }

        /// <summary>
        /// A datestamp which indicates when a document should be published, used in automated publish/unpublish scenarios
        /// </summary>
        [Obsolete("Obsolete, Use ReleaseDate property on Umbraco.Core.Models.Content", false)]
        public DateTime ReleaseDate
        {
            get { return _release; }
            set
            {
                _release = value;
                ContentEntity.ReleaseDate = value;
            }
        }

        /// <summary>
        /// A datestamp which indicates when a document should be unpublished, used in automated publish/unpublish scenarios
        /// </summary>
        [Obsolete("Obsolete, Use ExpireDate property on Umbraco.Core.Models.Content", false)]
        public DateTime ExpireDate
        {
            get { return _expire; }
            set
            {
                _expire = value;
                ContentEntity.ExpireDate = value;
            }
        }

        /// <summary>
        /// The id of the template associated to the document
        /// 
        /// When a document is created, it will get have default template given by it's documenttype,
        /// an editor is able to assign alternative templates (allowed by it's the documenttype)
        /// 
        /// You are always able to override the template in the runtime by appending the following to the querystring to the Url:
        /// 
        /// ?altTemplate=[templatealias]
        /// </summary>
        [Obsolete("Obsolete, Use Template property on Umbraco.Core.Models.Content", false)]
        public int Template
        {
            get { return _template; }
            set
            {
                _template = value;
                if (value == 0)
                {
                    ContentEntity.Template = null;
                }
                else
                {
                    var template = ApplicationContext.Current.Services.FileService.GetTemplate(value);
                    ContentEntity.Template = template;
                }
            }
        }

        /// <summary>
        /// A collection of documents imidiately underneath this document ie. the childdocuments
        /// </summary>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentService.GetChildren()", false)]
        public new Document[] Children
        {
            get
            {
                //cache the documents children so that this db call doesn't have to occur again
                if (_children == null)
                    _children = GetChildrenForTree(Id);

                return _children.ToArray();
            }
        }

        #endregion

        #region Public Methods
        

        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentService.PublishWithChildren()", false)]
        public bool PublishWithChildrenWithResult(IUser u)
        {
            var result = ApplicationContext.Current.Services.ContentService.PublishWithChildrenWithStatus(ContentEntity, u.Id, true);
            //This used to just return false only when the parent content failed, otherwise would always return true so we'll
            // do the same thing for the moment
            return result.Single(x => x.Result.ContentItem.Id == Id).Success;
        }
        

        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentService.UnPublish()", false)]
        public void UnPublish()
        {
            _published = ApplicationContext.Current.Services.ContentService.UnPublish(ContentEntity);
        }      

        
        /// <summary>
        /// Puts the current document in the trash
        /// </summary>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentService.MoveToRecycleBin()", false)]
        public override void delete()
        {
            MoveToTrash();
        }

        /// <summary>
        /// With either move the document to the trash or permanently remove it from the database.
        /// </summary>
        /// <param name="deletePermanently">flag to set whether or not to completely remove it from the database or just send to trash</param>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentService.Delete() or Umbraco.Core.Services.ContentService.MoveToRecycleBin()", false)]
        public void delete(bool deletePermanently)
        {
            if (!deletePermanently)
            {
                MoveToTrash();
            }
            else
            {
                DeletePermanently();
            }
        }

        public override List<CMSPreviewNode> GetNodesForPreview(bool childrenOnly)
        {
            var nodes = new List<CMSPreviewNode>();

            string pathExp = childrenOnly ? Path + ",%" : Path;

            IRecordsReader dr = SqlHelper.ExecuteReader(String.Format(SqlOptimizedForPreview, pathExp));
            while (dr.Read())
                nodes.Add(new CMSPreviewNode(dr.GetInt("id"), dr.GetGuid("versionId"), dr.GetInt("parentId"), dr.GetShort("level"), dr.GetInt("sortOrder"), dr.GetString("xml"), !dr.GetBoolean("published")));
            dr.Close();

            return nodes;
        }
        
        #endregion

        #region Protected Methods
        [Obsolete("Obsolete", false)]
        protected override void setupNode()
        {
            if (Id == -1 || Id == -20)
            {
                base.setupNode();
                return;
            }

            var content = Version == Guid.Empty
                           ? ApplicationContext.Current.Services.ContentService.GetById(Id)
                           : ApplicationContext.Current.Services.ContentService.GetByVersion(Version);

            if(content == null)
                throw new ArgumentException(string.Format("No Document exists with id '{0}'", Id));

            SetupNode(content);
        }

        private void SetupNode(IContent content)
        {
            ContentEntity = content;
            //Also need to set the ContentBase item to this one so all the propery values load from it
            ContentBase = ContentEntity;

            //Setting private properties from IContentBase replacing CMSNode.setupNode() / CMSNode.PopulateCMSNodeFromReader()
            base.PopulateCMSNodeFromUmbracoEntity(ContentEntity, _objectType);

            //If the version is empty we update with the latest version from the current IContent.
            if (Version == Guid.Empty)
                Version = ContentEntity.Version;

            //Setting private properties from IContent replacing Document.setupNode()
            _creator = ApplicationContext.Current.Services.UserService.GetUserById(ContentEntity.CreatorId);
            _writer = ApplicationContext.Current.Services.UserService.GetUserById(ContentEntity.WriterId);
            _updated = ContentEntity.UpdateDate;

            if (ContentEntity.Template != null)
                _template = ContentEntity.Template.Id;

            if (ContentEntity.ExpireDate.HasValue)
                _expire = ContentEntity.ExpireDate.Value;

            if (ContentEntity.ReleaseDate.HasValue)
                _release = ContentEntity.ReleaseDate.Value;

            _published = ContentEntity.Published;
        }

        [Obsolete("Obsolete", false)]
        protected void InitializeDocument(User InitUser, User InitWriter, string InitText, int InitTemplate,
                                          DateTime InitReleaseDate, DateTime InitExpireDate, DateTime InitUpdateDate,
                                          bool InitPublished)
        {
            if (InitUser == null)
            {
                throw new ArgumentNullException("InitUser");
            }
            if (InitWriter == null)
            {
                throw new ArgumentNullException("InitWriter");
            }
            _creator = InitUser;
            _writer = InitWriter;
            SetText(InitText);
            _template = InitTemplate;
            _release = InitReleaseDate;
            _expire = InitExpireDate;
            _updated = InitUpdateDate;
            _published = InitPublished;
        }

        #endregion

        #region Private Methods       

        /// <summary>
        /// Used internally to permanently delete the data from the database
        /// </summary>
        /// <returns>returns true if deletion isn't cancelled</returns>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentService.Delete()", false)]
        private bool DeletePermanently()
        {
            if (ContentEntity != null)
            {
                ApplicationContext.Current.Services.ContentService.Delete(ContentEntity);
            }
            else
            {
                ContentEntity = ApplicationContext.Current.Services.ContentService.GetById(Id);
                ApplicationContext.Current.Services.ContentService.Delete(ContentEntity);
            }

            //Keeping the base.delete() as it looks to be clear 'private/internal cache'
            base.delete();
            return true;
        }

        /// <summary>
        /// Used internally to move the node to the recyle bin
        /// </summary>
        /// <returns>Returns true if the move was not cancelled</returns>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentService.MoveToRecycleBin()", false)]
        private bool MoveToTrash()
        {

            UnPublish();
            if (ContentEntity != null)
            {
                ApplicationContext.Current.Services.ContentService.MoveToRecycleBin(ContentEntity);
            }
            else
            {
                ContentEntity = ApplicationContext.Current.Services.ContentService.GetById(Id);
                ApplicationContext.Current.Services.ContentService.MoveToRecycleBin(ContentEntity);
            }
            return true;
        }

        #endregion
        

    }
}
