using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.EntityBase;

using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using umbraco.BusinessLogic;
using umbraco.BusinessLogic.Actions;
using umbraco.cms.helpers;
using umbraco.DataLayer;
using Property = umbraco.cms.businesslogic.property.Property;
using Umbraco.Core.Strings;

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
        /// Initializes a Document object with one SQL query instead of many
        /// </summary>
        /// <param name="optimizedMode"></param>
        /// <param name="id"></param>
        public Document(bool optimizedMode, int id)
            : base(id, optimizedMode)
        {
            this._optimizedMode = optimizedMode;
            if (optimizedMode)
            {
                ContentEntity = ApplicationContext.Current.Services.ContentService.GetById(id);
                bool hasChildren = ApplicationContext.Current.Services.ContentService.HasChildren(id);
                int templateId = ContentEntity.Template == null ? 0 : ContentEntity.Template.Id;

                SetupDocumentForTree(ContentEntity.Key, ContentEntity.Level, ContentEntity.ParentId, ContentEntity.CreatorId,
                                     ContentEntity.WriterId,
                                     ContentEntity.Published, ContentEntity.Path, ContentEntity.Name, ContentEntity.CreateDate,
                                     ContentEntity.UpdateDate, ContentEntity.UpdateDate, ContentEntity.ContentType.Icon, hasChildren,
                                     ContentEntity.ContentType.Alias, ContentEntity.ContentType.Thumbnail,
                                     ContentEntity.ContentType.Description, null, ContentEntity.ContentType.Id,
                                     templateId, ContentEntity.ContentType.IsContainer);

                var tmpReleaseDate = ContentEntity.ReleaseDate.HasValue ? ContentEntity.ReleaseDate.Value : new DateTime();
                var tmpExpireDate = ContentEntity.ExpireDate.HasValue ? ContentEntity.ExpireDate.Value : new DateTime();
                var creator = new User(ContentEntity.CreatorId, true);
                var writer = new User(ContentEntity.WriterId, true);

                InitializeContent(ContentEntity.ContentType.Id, ContentEntity.Version, ContentEntity.UpdateDate,
                                  ContentEntity.ContentType.Icon);
                InitializeDocument(creator, writer, ContentEntity.Name, templateId, tmpReleaseDate, tmpExpireDate,
                                   ContentEntity.UpdateDate, ContentEntity.Published);
            }
        }

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
        private User _creator;
        private User _writer;
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
        /// Imports (create) a document from a xmlrepresentation of a document, used by the packager
        /// </summary>
        /// <param name="ParentId">The id to import to</param>
        /// <param name="Creator">Creator of the new document</param>
        /// <param name="Source">Xmlsource</param>
        public static int Import(int ParentId, User Creator, XmlElement Source)
        {
            // check what schema is used for the xml
            bool sourceIsLegacySchema = Source.Name.ToLower() == "node" ? true : false;

            // check whether or not to create a new document
            int id = int.Parse(Source.GetAttribute("id"));
            Document d = null;
            if (Document.IsDocument(id))
            {
                try
                {
                    // if the parent is the same, we'll update the existing document. Else we'll create a new document below
                    d = new Document(id);
                    if (d.ParentId != ParentId)
                        d = null;
                }
                catch { }
            }

            // document either didn't exist or had another parent so we'll create a new one
            if (d == null)
            {
                string nodeTypeAlias = sourceIsLegacySchema ? Source.GetAttribute("nodeTypeAlias") : Source.Name;
                d = MakeNew(
                    Source.GetAttribute("nodeName"),
                    DocumentType.GetByAlias(nodeTypeAlias),
                    Creator,
                    ParentId);
            }
            else
            {
                // update name of the document
                d.Text = Source.GetAttribute("nodeName");
            }

            d.CreateDateTime = DateTime.Parse(Source.GetAttribute("createDate"));

            // Properties
            string propertyXPath = sourceIsLegacySchema ? "data" : "* [not(@isDoc)]";
            foreach (XmlElement n in Source.SelectNodes(propertyXPath))
            {
                string propertyAlias = sourceIsLegacySchema ? n.GetAttribute("alias") : n.Name;
                Property prop = d.getProperty(propertyAlias);
                string propValue = xmlHelper.GetNodeValue(n);

                if (prop != null)
                {
                    // only update real values
                    if (!String.IsNullOrEmpty(propValue))
                    {
                        //test if the property has prevalues, of so, try to convert the imported values so they match the new ones
                        SortedList prevals = cms.businesslogic.datatype.PreValues.GetPreValues(prop.PropertyType.DataTypeDefinition.Id);

                        //Okey we found some prevalue, let's replace the vals with some ids
                        if (prevals.Count > 0)
                        {
                            System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>(propValue.Split(','));

                            foreach (DictionaryEntry item in prevals)
                            {
                                string pval = ((umbraco.cms.businesslogic.datatype.PreValue)item.Value).Value;
                                string pid = ((umbraco.cms.businesslogic.datatype.PreValue)item.Value).Id.ToString();

                                if (list.Contains(pval))
                                    list[list.IndexOf(pval)] = pid;

                            }

                            //join the list of new values and return it as the new property value
                            System.Text.StringBuilder builder = new System.Text.StringBuilder();
                            bool isFirst = true;

                            foreach (string str in list)
                            {
                                if (!isFirst)
                                    builder.Append(",");

                                builder.Append(str);
                                isFirst = false;
                            }
                            prop.Value = builder.ToString();

                        }
                        else
                            prop.Value = propValue;
                    }
                }
                else
                {
					LogHelper.Warn<Document>(String.Format("Couldn't import property '{0}' as the property type doesn't exist on this document type", propertyAlias));
                }
            }

            d.Save();

            // Subpages
            string subXPath = sourceIsLegacySchema ? "node" : "* [@isDoc]";
            foreach (XmlElement n in Source.SelectNodes(subXPath))
                Import(d.Id, Creator, n);

            return d.Id;
        }

        /// <summary>
        /// Creates a new document
        /// </summary>
        /// <param name="Name">The name (.Text property) of the document</param>
        /// <param name="dct">The documenttype</param>
        /// <param name="u">The usercontext under which the action are performed</param>
        /// <param name="ParentId">The id of the parent to the document</param>
        /// <returns>The newly created document</returns>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentService.CreateContent()", false)]
        public static Document MakeNew(string Name, DocumentType dct, User u, int ParentId)
        {
            //allows you to cancel a document before anything goes to the DB
            var newingArgs = new DocumentNewingEventArgs()
                                 {
                                     Text = Name,
                                     DocumentType = dct,
                                     User = u,
                                     ParentId = ParentId
                                 };
            Document.OnNewing(newingArgs);
            if (newingArgs.Cancel)
            {
                return null;
            }

            //Create a new IContent object based on the passed in DocumentType's alias, set the name and save it
            IContent content = ApplicationContext.Current.Services.ContentService.CreateContentWithIdentity(Name, ParentId, dct.Alias, u.Id);
            //The content object will only have the 'WasCancelled' flag set to 'True' if the 'Creating' event has been cancelled, so we return null.
            if (((Entity)content).WasCancelled)
                return null;

            //read the whole object from the db
            Document d = new Document(content);

            //event
            NewEventArgs e = new NewEventArgs();
            d.OnNew(e);

            // Log
            LogHelper.Info<Document>(string.Format("New document {0}", d.Id));

            // Save doc
            d.Save();

            return d;
        }

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

        public static int CountSubs(int parentId, bool publishedOnly)
        {
            if (!publishedOnly)
            {
                return CountSubs(parentId);
            }

            return ApplicationContext.Current.DatabaseContext.Database.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM (select distinct umbracoNode.id from umbracoNode INNER JOIN cmsDocument ON cmsDocument.published = 1 and cmsDocument.nodeId = umbracoNode.id WHERE ','+path+',' LIKE '%," +
                parentId.ToString() + ",%') t");
        }

        /// <summary>
        /// Deletes all documents of a type, will be invoked if a documenttype is deleted.
        /// 
        /// Note: use with care: this method can result in wast amount of data being deleted.
        /// </summary>
        /// <param name="dt">The type of which documents should be deleted</param>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentService.DeleteContentOfType()", false)]
        public static void DeleteFromType(DocumentType dt)
        {
            ApplicationContext.Current.Services.ContentService.DeleteContentOfType(dt.Id);
        }

        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentService.GetContentOfContentType()", false)]
        public static IEnumerable<Document> GetDocumentsOfDocumentType(int docTypeId)
        {
            var contents = ApplicationContext.Current.Services.ContentService.GetContentOfContentType(docTypeId);
            return contents.Select(x => new Document(x)).ToArray();
        }

        public static void RemoveTemplateFromDocument(int templateId)
        {
            ApplicationContext.Current.DatabaseContext.Database.Execute(
                "update cmsDocument set templateId = NULL where templateId = @TemplateId", new {TemplateId = templateId});
            //We need to clear cache for Documents since this is touching the database directly
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheObjectTypes<IContent>();
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

        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentService.GetChildrenByName()", false)]
        public static List<Document> GetChildrenBySearch(int NodeId, string searchString)
        {
            var children = ApplicationContext.Current.Services.ContentService.GetChildrenByName(NodeId, searchString);
            return children.Select(x => new Document(x)).ToList();
        }
                
        /// <summary>
        /// This will clear out the cmsContentXml table for all Documents (not media or members) and then
        /// rebuild the xml for each Docuemtn item and store it in this table.
        /// </summary>
        /// <remarks>
        /// This method is thread safe
        /// </remarks>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentService.RePublishAll()", false)]
        public static void RePublishAll()
        {
            ApplicationContext.Current.Services.ContentService.RePublishAll();
        }

        public static void RegeneratePreviews()
        {
            XmlDocument xd = new XmlDocument();
            IRecordsReader dr = SqlHelper.ExecuteReader("select nodeId from cmsDocument");

            while (dr.Read())
            {
                try
                {
                    new Document(dr.GetInt("nodeId")).SaveXmlPreview(xd);
                }
                catch (Exception ee)
                {
					LogHelper.Error<Document>("Error generating preview xml", ee);
                }
            }
            dr.Close();
        }

        /// <summary>
        /// Retrieve a list of documents with an expirationdate greater than today
        /// </summary>
        /// <returns>A list of documents with expirationdates than today</returns>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentService.GetContentForExpiration()", false)]
        public static Document[] GetDocumentsForExpiration()
        {
            var contents = ApplicationContext.Current.Services.ContentService.GetContentForExpiration();
            return contents.Select(x => new Document(x)).ToArray();
        }

        /// <summary>
        /// Retrieve a list of documents with with releasedate greater than today
        /// </summary>
        /// <returns>Retrieve a list of documents with with releasedate greater than today</returns>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentService.GetContentForRelease()", false)]
        public static Document[] GetDocumentsForRelease()
        {
            var contents = ApplicationContext.Current.Services.ContentService.GetContentForRelease();
            return contents.Select(x => new Document(x)).ToArray();
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
            get
            {
                return ContentEntity == null ? base.ParentId : ContentEntity.ParentId;
            }
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
        public User Creator
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
        public User Writer
        {
            get
            {
                if (_writer == null)
                {
                    if (!_writerId.HasValue)
                    {
                        throw new NullReferenceException("Writer ID has not been specified for this document");
                    }
                    _writer = User.GetUser(_writerId.Value);
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

        /// <summary>
        /// Saves and executes handlers and events for the Send To Publication action.
        /// </summary>
        /// <param name="u">The User</param>
        public bool SendToPublication(User u)
        {
            var e = new SendToPublishEventArgs();
            FireBeforeSendToPublish(e);
            if (e.Cancel == false)
            {
                var sent = ApplicationContext.Current.Services.ContentService.SendToPublication(ContentEntity, u.Id);
                if (sent)
                {
                    FireAfterSendToPublish(e);
                    return true;    
                }
            }

            return false;

        }

        /// <summary>
        /// Saves and Publishes a document.
        /// A xmlrepresentation of the document and its data are exposed to the runtime data
        /// (an xmlrepresentation is added -or updated if the document previously are published) ,
        /// this will lead to a new version of the document being created, for continuing editing of
        /// the data.
        /// </summary>
        /// <param name="u">The usercontext under which the action are performed</param>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentService.Publish()", false)]
        public void Publish(User u)
        {
            this.Published = SaveAndPublish(u);
        }

        /// <summary>
        /// Publishing a document
        /// A xmlrepresentation of the document and its data are exposed to the runtime data
        /// (an xmlrepresentation is added -or updated if the document previously are published) ,
        /// this will lead to a new version of the document being created, for continuing editing of
        /// the data.
        /// </summary>
        /// <param name="u">The usercontext under which the action are performed</param>
        /// <returns>True if the publishing succeed. Possible causes for not publishing is if an event aborts the publishing</returns>
        /// <remarks>        
        /// This method needs to be marked with [MethodImpl(MethodImplOptions.Synchronized)]
        /// because we execute multiple queries affecting the same data, if two thread are to do this at the same time for the same node we may have problems
        /// </remarks>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentService.Publish()", false)]
        public bool PublishWithResult(User u)
        {
            return PublishWithResult(u, true);
        }

        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentService.Publish()", false)]
        internal bool PublishWithResult(User u, bool omitCacheRefresh)
        {
            var e = new PublishEventArgs();
            FireBeforePublish(e);

            if (!e.Cancel)
            {
                var result = ApplicationContext.Current.Services.ContentService.PublishWithStatus(ContentEntity, u.Id);
                _published = result.Success;
                
                FireAfterPublish(e);

                return result.Success;
            }
            else
            {
                return false;
            }
        }

        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentService.PublishWithChildren()", false)]
        public bool PublishWithChildrenWithResult(User u)
        {
            var result = ApplicationContext.Current.Services.ContentService.PublishWithChildrenWithStatus(ContentEntity, u.Id, true);
            //This used to just return false only when the parent content failed, otherwise would always return true so we'll
            // do the same thing for the moment
            return result.Single(x => x.Result.ContentItem.Id == Id).Success;
        }

        /// <summary>
        /// Rollbacks a document to a previous version, this will create a new version of the document and copy
        /// all of the old documents data.
        /// </summary>
        /// <param name="u">The usercontext under which the action are performed</param>
        /// <param name="VersionId">The unique Id of the version to roll back to</param>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentService.Rollback()", false)]
        public void RollBack(Guid VersionId, User u)
        {
            var e = new RollBackEventArgs();
            FireBeforeRollBack(e);

            if (!e.Cancel)
            {
                ContentEntity = ApplicationContext.Current.Services.ContentService.Rollback(Id, VersionId, u.Id);

                FireAfterRollBack(e);
            }
        }

        /// <summary>
        /// Recursive publishing.
        /// 
        /// Envoking this method will publish the documents and all children recursive.
        /// </summary>
        /// <param name="u">The usercontext under which the action are performed</param>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentService.PublishWithChildren()", false)]
        public void PublishWithSubs(User u)
        {
            PublishEventArgs e = new PublishEventArgs();
            FireBeforePublish(e);

            if (!e.Cancel)
            {
                IEnumerable<Attempt<PublishStatus>> publishedResults = ApplicationContext.Current.Services.ContentService
                    .PublishWithChildrenWithStatus(ContentEntity, u.Id);

                FireAfterPublish(e);
            }
        }

        [Obsolete("Don't use! Only used internally to support the legacy events", false)]
        internal IEnumerable<Attempt<PublishStatus>> PublishWithSubs(int userId, bool includeUnpublished)
        {
            PublishEventArgs e = new PublishEventArgs();
            FireBeforePublish(e);

            IEnumerable<Attempt<PublishStatus>> publishedResults = Enumerable.Empty<Attempt<PublishStatus>>();

            if (!e.Cancel)
            {
                publishedResults = ApplicationContext.Current.Services.ContentService
                    .PublishWithChildrenWithStatus(ContentEntity, userId, includeUnpublished);

                FireAfterPublish(e);
            }

            return publishedResults;
        }

        [Obsolete("Don't use! Only used internally to support the legacy events", false)]
        internal Attempt<PublishStatus> SaveAndPublish(int userId)
        {
            var result = Attempt.Fail(new PublishStatus(ContentEntity, PublishStatusType.FailedCancelledByEvent));
            foreach (var property in GenericProperties)
            {
                ContentEntity.SetValue(property.PropertyType.Alias, property.Value);
            }

            var saveArgs = new SaveEventArgs();
            FireBeforeSave(saveArgs);

            if (!saveArgs.Cancel)
            {
                var publishArgs = new PublishEventArgs();
                FireBeforePublish(publishArgs);

                if (!publishArgs.Cancel)
                {
                    //NOTE: The 'false' parameter will cause the PublishingStrategy events to fire which will ensure that the cache is refreshed.
                    result = ApplicationContext.Current.Services.ContentService
                        .SaveAndPublishWithStatus(ContentEntity, userId);
                    base.VersionDate = ContentEntity.UpdateDate;
                    this.UpdateDate = ContentEntity.UpdateDate;

                    //NOTE: This is just going to call the CMSNode Save which will launch into the CMSNode.BeforeSave and CMSNode.AfterSave evenths
                    // which actually do dick all and there's no point in even having them there but just in case for some insane reason someone
                    // has bound to those events, I suppose we'll need to keep this here.
                    base.Save();

                    //Launch the After Save event since we're doing 2 things in one operation: Saving and publishing.
                    FireAfterSave(saveArgs);

                    //Now we need to fire the After publish event
                    FireAfterPublish(publishArgs);
                }
            }

            return result;
        }

        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentService.UnPublish()", false)]
        public void UnPublish()
        {
            UnPublishEventArgs e = new UnPublishEventArgs();

            FireBeforeUnPublish(e);

            if (!e.Cancel)
            {
                _published = ApplicationContext.Current.Services.ContentService.UnPublish(ContentEntity);
                
                FireAfterUnPublish(e);
            }
        }

        /// <summary>
        /// Used to persist object changes to the database. 
        /// </summary>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentService.Save()", false)]
        public override void Save()
        {
            var e = new SaveEventArgs();
            FireBeforeSave(e);

            foreach (var property in GenericProperties)
            {
                ContentEntity.SetValue(property.PropertyType.Alias, property.Value);
            }

            if (!e.Cancel)
            {
                var current = User.GetCurrent();
                int userId = current == null ? 0 : current.Id;
                ApplicationContext.Current.Services.ContentService.Save(ContentEntity, userId);

                base.VersionDate = ContentEntity.UpdateDate;
                this.UpdateDate = ContentEntity.UpdateDate;

                base.Save();

                FireAfterSave(e);
            }
        }

        /// <summary>
        /// Do not use! only used internally in order to get the published status until we upgrade everything to use the new API
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        [Obsolete("Do not use! only used internally in order to get the published status until we upgrade everything to use the new API")]
        internal Attempt<PublishStatus> SaveAndPublishWithResult(User u)
        {
            foreach (var property in GenericProperties)
            {
                ContentEntity.SetValue(property.PropertyType.Alias, property.Value);
            }

            var saveArgs = new SaveEventArgs();
            FireBeforeSave(saveArgs);

            if (!saveArgs.Cancel)
            {
                var publishArgs = new PublishEventArgs();
                FireBeforePublish(publishArgs);

                if (!publishArgs.Cancel)
                {
                    //NOTE: The 'false' parameter will cause the PublishingStrategy events to fire which will ensure that the cache is refreshed.
                    var result = ApplicationContext.Current.Services.ContentService
                        .SaveAndPublishWithStatus(ContentEntity, u.Id);
                    base.VersionDate = ContentEntity.UpdateDate;
                    this.UpdateDate = ContentEntity.UpdateDate;

                    //NOTE: This is just going to call the CMSNode Save which will launch into the CMSNode.BeforeSave and CMSNode.AfterSave evenths
                    // which actually do dick all and there's no point in even having them there but just in case for some insane reason someone
                    // has bound to those events, I suppose we'll need to keep this here.
                    base.Save();

                    //Launch the After Save event since we're doing 2 things in one operation: Saving and publishing.
                    FireAfterSave(saveArgs);

                    //Now we need to fire the After publish event
                    FireAfterPublish(publishArgs);

                    return result;
                }

                return new Attempt<PublishStatus>(false, new PublishStatus(ContentEntity, PublishStatusType.FailedCancelledByEvent));
            }

            return new Attempt<PublishStatus>(false, new PublishStatus(ContentEntity, PublishStatusType.FailedCancelledByEvent));
        }

        /// <summary>
        /// Saves and publishes a document
        /// </summary>
        /// <param name="u">The usercontext under which the action are performed</param>
        /// <returns></returns>
        public bool SaveAndPublish(User u)
        {
            var result = SaveAndPublishWithResult(u);
            return result.Success;
        }

        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentService.HasPublishedVersion()", false)]
        public bool HasPublishedVersion()
        {
            return ContentEntity.HasPublishedVersion;
        }

        /// <summary>
        /// Pending changes means that there have been property/data changes since the last published version.
        /// This is determined by the comparing the version date to the updated date. if they are different by more than 2 seconds, 
        /// then this is considered a change.
        /// </summary>
        /// <returns></returns>
        [Obsolete("Obsolete, Instead of calling this just check if the latest version of the content is published", false)]
        public bool HasPendingChanges()
        {
            return ContentEntity.Published == false && ((Umbraco.Core.Models.Content)ContentEntity).PublishedState != PublishedState.Unpublished;
        }

        /// <summary>
        /// Used for rolling back documents to a previous version
        /// </summary>
        /// <returns> Previous published versions of the document</returns>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentService.GetVersions()", false)]
        public DocumentVersionList[] GetVersions()
        {
            var versions = ApplicationContext.Current.Services.ContentService.GetVersions(Id);
            return
                versions.Select(x => new DocumentVersionList(x.Version, x.UpdateDate, x.Name, User.GetUser(x.CreatorId)))
                        .ToArray();
        }

        /// <summary>
        /// Returns the published version of this document
        /// </summary>
        /// <returns>The published version of this document</returns>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentService.GetPublishedVersion()", false)]
        public DocumentVersionList GetPublishedVersion()
        {
            var version = ApplicationContext.Current.Services.ContentService.GetPublishedVersion(Id);
            if (version == null)
                return null;

            return new DocumentVersionList(version.Version, version.UpdateDate, version.Name, User.GetUser(version.CreatorId));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Returns a breadcrumlike path for the document like: /ancestorname/ancestorname</returns>
        [Obsolete("Method is not used anywhere, so its marked for deletion")]
        public string GetTextPath()
        {
            string tempPath = "";
            string[] splitPath = Path.Split(".".ToCharArray());
            for (int i = 1; i < Level; i++)
            {
                tempPath += new Document(int.Parse(splitPath[i])).Text + "/";
            }
            if (tempPath.Length > 0)
                tempPath = tempPath.Substring(0, tempPath.Length - 1);
            return tempPath;
        }

        /// <summary>
        /// Overrides the moving of a <see cref="Document"/> object to a new location by changing its parent id.
        /// </summary>
        public override void Move(int newParentId)
        {
            MoveEventArgs e = new MoveEventArgs();
            base.FireBeforeMove(e);

            if (!e.Cancel)
            {
                var current = User.GetCurrent();
                int userId = current == null ? 0 : current.Id;
                ApplicationContext.Current.Services.ContentService.Move(ContentEntity, newParentId, userId);

                //We need to manually update this property as the above change is not directly reflected in 
                //the current object unless its reloaded.
                base.ParentId = newParentId;
            }

            base.FireAfterMove(e);
        }

        /// <summary>
        /// Creates a new document of the same type and copies all data from the current onto it. Due to backwards compatibility we can't return
        /// the new Document, but it's included in the CopyEventArgs.Document if you subscribe to the AfterCopy event
        /// </summary>
        /// <param name="CopyTo">The parentid where the document should be copied to</param>
        /// <param name="u">The usercontext under which the action are performed</param>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentService.Copy()", false)]
        public Document Copy(int CopyTo, User u)
        {
            return Copy(CopyTo, u, false);
        }

        /// <summary>
        /// Creates a new document of the same type and copies all data from the current onto it. Due to backwards compatibility we can't return
        /// the new Document, but it's included in the CopyEventArgs.Document if you subscribe to the AfterCopy event
        /// </summary>
        /// <param name="CopyTo"></param>
        /// <param name="u"></param>
        /// <param name="RelateToOrignal"></param>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentService.Copy()", false)]
        public Document Copy(int CopyTo, User u, bool RelateToOrignal)
        {
            var e = new CopyEventArgs();
            e.CopyTo = CopyTo;
            FireBeforeCopy(e);
            Document newDoc = null;

            if (!e.Cancel)
            {
                // Make the new document
                var content = ApplicationContext.Current.Services.ContentService.Copy(ContentEntity, CopyTo, RelateToOrignal, u.Id);
                newDoc = new Document(content);
                
                // Then save to preserve any changes made by action handlers
                newDoc.Save();

                e.NewDocument = newDoc;
                FireAfterCopy(e);
            }

            return newDoc;
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

        /// <summary>
        /// Returns all decendants of the current document
        /// </summary>
        /// <returns></returns>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentService.GetDescendants()", false)]
        public override IEnumerable GetDescendants()
        {
            var descendants = ContentEntity == null
                                  ? ApplicationContext.Current.Services.ContentService.GetDescendants(Id)
                                  : ApplicationContext.Current.Services.ContentService.GetDescendants(ContentEntity);

            return descendants.Select(x => new Document(x.Id, true));
        }

        /// <summary>
        /// Refreshes the xml, used when publishing data on a document which already is published
        /// </summary>
        /// <param name="xd">The source xmldocument</param>
        /// <param name="x">The previous xmlrepresentation of the document</param>
        [Obsolete("Obsolete, Doesn't appear to be used anywhere", false)]
        public void XmlNodeRefresh(XmlDocument xd, ref XmlNode x)
        {
            x.Attributes.RemoveAll();
            foreach (XmlNode xDel in x.SelectNodes("./data"))
                x.RemoveChild(xDel);

            XmlPopulate(xd, ref x, false);
        }

        /// <summary>
        /// Creates an xmlrepresentation of the document and saves it to the database
        /// </summary>
        /// <param name="xd"></param>
        public override void XmlGenerate(XmlDocument xd)
        {
            XmlNode x = generateXmlWithoutSaving(xd);
            // Save to db
            saveXml(x);
        }

        /// <summary>
        /// A xmlrepresentaion of the document, used when publishing/exporting the document, 
        /// 
        /// Optional: Recursive get childdocuments xmlrepresentation
        /// </summary>
        /// <param name="xd">The xmldocument</param>
        /// <param name="Deep">Recursive add of childdocuments</param>
        /// <returns></returns>
        public override XmlNode ToXml(XmlDocument xd, bool Deep)
        {
            if (Published)
            {
                if (_xml == null)
                {
                    // Load xml from db if _xml hasn't been loaded yet
                    _xml = importXml();

                    // Generate xml if xml still null (then it hasn't been initialized before)
                    if (_xml == null)
                    {
                        XmlGenerate(new XmlDocument());
                        _xml = importXml();
                    }
                }

                XmlNode x = xd.ImportNode(_xml, true);

                if (Deep)
                {
                    var c = Children;
                    foreach (Document d in c)
                    {
                        if (d.Published)
                            x.AppendChild(d.ToXml(xd, true));
                    }
                }

                return x;
            }
            else
                return null;
        }

        /// <summary>
        /// Populate a documents xmlnode
        /// </summary>
        /// <param name="xd">Xmldocument context</param>
        /// <param name="x">The node to fill with data</param>
        /// <param name="Deep">If true the documents childrens xmlrepresentation will be appended to the Xmlnode recursive</param>
        public override void XmlPopulate(XmlDocument xd, ref XmlNode x, bool Deep)
        {
            string urlName = this.ContentEntity.GetUrlSegment().ToLower();
            foreach (Property p in GenericProperties.Where(p => p != null && p.Value != null && string.IsNullOrEmpty(p.Value.ToString()) == false))
                x.AppendChild(p.ToXml(xd));

            // attributes
            x.Attributes.Append(addAttribute(xd, "id", Id.ToString()));
            //            x.Attributes.Append(addAttribute(xd, "version", Version.ToString()));
            if (Level > 1)
                x.Attributes.Append(addAttribute(xd, "parentID", Parent.Id.ToString()));
            else
                x.Attributes.Append(addAttribute(xd, "parentID", "-1"));
            x.Attributes.Append(addAttribute(xd, "level", Level.ToString()));
            x.Attributes.Append(addAttribute(xd, "writerID", Writer.Id.ToString()));
            x.Attributes.Append(addAttribute(xd, "creatorID", Creator.Id.ToString()));
            if (ContentType != null)
                x.Attributes.Append(addAttribute(xd, "nodeType", ContentType.Id.ToString()));
            x.Attributes.Append(addAttribute(xd, "template", _template.ToString()));
            x.Attributes.Append(addAttribute(xd, "sortOrder", sortOrder.ToString()));
            x.Attributes.Append(addAttribute(xd, "createDate", CreateDateTime.ToString("s")));
            x.Attributes.Append(addAttribute(xd, "updateDate", VersionDate.ToString("s")));
            x.Attributes.Append(addAttribute(xd, "nodeName", Text));
            x.Attributes.Append(addAttribute(xd, "urlName", urlName));
            x.Attributes.Append(addAttribute(xd, "writerName", Writer.Name));
            x.Attributes.Append(addAttribute(xd, "creatorName", Creator.Name.ToString()));
            if (ContentType != null && UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema)
                x.Attributes.Append(addAttribute(xd, "nodeTypeAlias", ContentType.Alias));
            x.Attributes.Append(addAttribute(xd, "path", Path));

            if (!UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema)
            {
                x.Attributes.Append(addAttribute(xd, "isDoc", ""));
            }

            if (Deep)
            {
                //store children array here because iterating over an Array object is very inneficient.
                var c = Children;
                foreach (Document d in c)
                {
                    XmlNode xml = d.ToXml(xd, true);
                    if (xml != null)
                    {
                        x.AppendChild(xml);
                    }
                    else
                    {
                        LogHelper.Debug<Document>(string.Format("Document {0} not published so XML cannot be generated", d.Id));
                    }
                }

            }
        }

        /// <summary>
        /// This is a specialized method which literally just makes sure that the sortOrder attribute of the xml
        /// that is stored in the database is up to date.
        /// </summary>
        public void refreshXmlSortOrder()
        {
            if (Published)
            {
                if (_xml == null)
                    // Load xml from db if _xml hasn't been loaded yet
                    _xml = importXml();

                // Generate xml if xml still null (then it hasn't been initialized before)
                if (_xml == null)
                {
                    XmlGenerate(new XmlDocument());
                    _xml = importXml();
                }
                else
                {
                    // Update the sort order attr
                    _xml.Attributes.GetNamedItem("sortOrder").Value = sortOrder.ToString();
                    saveXml(_xml);
                }

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

        public override XmlNode ToPreviewXml(XmlDocument xd)
        {
            if (!PreviewExists(Version))
            {
                SaveXmlPreview(xd);
            }
            return GetPreviewXml(xd, Version);
        }

        /// <summary>
        /// Method to remove an assigned template from a document
        /// </summary>
        public void RemoveTemplate()
        {
            Template = 0;
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
            _creator = User.GetUser(ContentEntity.CreatorId);
            _writer = User.GetUser(ContentEntity.WriterId);
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

        [Obsolete("Obsolete", false)]
        protected void PopulateDocumentFromReader(IRecordsReader dr)
        {
            var hc = dr.GetInt("children") > 0;

            SetupDocumentForTree(dr.GetGuid("uniqueId")
                , dr.GetShort("level")
                , dr.GetInt("parentId")
                , dr.GetInt("nodeUser")
                , dr.GetInt("documentUser")
                , !dr.GetBoolean("trashed") && (dr.GetInt("isPublished") == 1) //set published... double check trashed property
                , dr.GetString("path")
                , dr.GetString("text")
                , dr.GetDateTime("createDate")
                , dr.GetDateTime("updateDate")
                , dr.GetDateTime("versionDate")
                , dr.GetString("icon")
                , hc
                , dr.GetString("alias")
                , dr.GetString("thumbnail")
                , dr.GetString("description")
                     , null
                , dr.GetInt("contentTypeId")
                     , dr.GetInt("templateId")
                     , dr.GetBoolean("isContainer"));

            if (!dr.IsNull("releaseDate"))
                _release = dr.GetDateTime("releaseDate");
            if (!dr.IsNull("expireDate"))
                _expire = dr.GetDateTime("expireDate");
        }

        protected void SaveXmlPreview(XmlDocument xd)
        {
            SavePreviewXml(generateXmlWithoutSaving(xd), Version);
        }

        #endregion

        #region Private Methods
        [Obsolete("Obsolete", false)]
        private void SetupDocumentForTree(Guid uniqueId, int level, int parentId, int creator, int writer, bool publish, string path,
                                         string text, DateTime createDate, DateTime updateDate,
                                         DateTime versionDate, string icon, bool hasChildren, string contentTypeAlias, string contentTypeThumb,
                                           string contentTypeDesc, int? masterContentType, int contentTypeId, int templateId, bool isContainer)
        {
            SetupNodeForTree(uniqueId, _objectType, level, parentId, creator, path, text, createDate, hasChildren);

            _writerId = writer;
            _published = publish;
            _updated = updateDate;
            _template = templateId;
            ContentType = new ContentType(contentTypeId, contentTypeAlias, icon, contentTypeThumb, null, isContainer);
            ContentTypeIcon = icon;
            VersionDate = versionDate;
        }

        private XmlAttribute addAttribute(XmlDocument Xd, string Name, string Value)
        {
            XmlAttribute temp = Xd.CreateAttribute(Name);
            temp.Value = Value;
            return temp;
        }

        /// <summary>
        /// This needs to be synchronized since we're doing multiple sql operations in the single method
        /// </summary>
        /// <param name="x"></param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void saveXml(XmlNode x)
        {
            bool exists = (SqlHelper.ExecuteScalar<int>("SELECT COUNT(nodeId) FROM cmsContentXml WHERE nodeId=@nodeId",
                                            SqlHelper.CreateParameter("@nodeId", Id)) != 0);
            string sql = exists ? "UPDATE cmsContentXml SET xml = @xml WHERE nodeId=@nodeId"
                                : "INSERT INTO cmsContentXml(nodeId, xml) VALUES (@nodeId, @xml)";
            SqlHelper.ExecuteNonQuery(sql,
                                      SqlHelper.CreateParameter("@nodeId", Id),
                                      SqlHelper.CreateParameter("@xml", x.OuterXml));
        }

        private XmlNode importXml()
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlReader xmlRdr = SqlHelper.ExecuteXmlReader(string.Format(
                                                       "select xml from cmsContentXml where nodeID = {0}", Id));
            xmlDoc.Load(xmlRdr);

            return xmlDoc.FirstChild;
        }

        /// <summary>
        /// Used internally to permanently delete the data from the database
        /// </summary>
        /// <returns>returns true if deletion isn't cancelled</returns>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentService.Delete()", false)]
        private bool DeletePermanently()
        {
            DeleteEventArgs e = new DeleteEventArgs();

            FireBeforeDelete(e);

            if (!e.Cancel)
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

                FireAfterDelete(e);
            }
            return !e.Cancel;
        }

        /// <summary>
        /// Used internally to move the node to the recyle bin
        /// </summary>
        /// <returns>Returns true if the move was not cancelled</returns>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentService.MoveToRecycleBin()", false)]
        private bool MoveToTrash()
        {
            MoveToTrashEventArgs e = new MoveToTrashEventArgs();
            FireBeforeMoveToTrash(e);

            if (!e.Cancel)
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
                FireAfterMoveToTrash(e);
            }
            return !e.Cancel;
        }

        #endregion

        #region Events

        /// <summary>
        /// The save event handler
        /// </summary>
        public delegate void SaveEventHandler(Document sender, SaveEventArgs e);
        /// <summary>
        /// The New event handler
        /// </summary>
        public delegate void NewEventHandler(Document sender, NewEventArgs e);
        /// <summary>
        /// The delete  event handler
        /// </summary>
        public delegate void DeleteEventHandler(Document sender, DeleteEventArgs e);
        /// <summary>
        /// The publish event handler
        /// </summary>
        public delegate void PublishEventHandler(Document sender, PublishEventArgs e);
        /// <summary>
        /// The Send To Publish event handler
        /// </summary>
        public delegate void SendToPublishEventHandler(Document sender, SendToPublishEventArgs e);
        /// <summary>
        /// The unpublish event handler
        /// </summary>
        public delegate void UnPublishEventHandler(Document sender, UnPublishEventArgs e);
        /// <summary>
        /// The copy event handler
        /// </summary>
        public delegate void CopyEventHandler(Document sender, CopyEventArgs e);
        /// <summary>
        /// The rollback event handler
        /// </summary>
        public delegate void RollBackEventHandler(Document sender, RollBackEventArgs e);

        /// <summary>
        /// The Move to trash event handler
        /// </summary>
        public delegate void MoveToTrashEventHandler(Document sender, MoveToTrashEventArgs e);

        /// <summary>
        /// Occurs when [before save].
        /// </summary>
        public new static event SaveEventHandler BeforeSave;
        /// <summary>
        /// Raises the <see cref="E:BeforeSave"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected internal new virtual void FireBeforeSave(SaveEventArgs e)
        {
            if (BeforeSave != null)
            {
                BeforeSave(this, e);
            }
        }

        /// <summary>
        /// Occurs when [after save].
        /// </summary>
        public new static event SaveEventHandler AfterSave;
        /// <summary>
        /// Raises the <see cref="E:AfterSave"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected new virtual void FireAfterSave(SaveEventArgs e)
        {
            if (AfterSave != null)
            {
                AfterSave(new Document(this.Id), e);
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

        //TODO: Slace - Document this
        public static event EventHandler<DocumentNewingEventArgs> Newing;
        protected static void OnNewing(DocumentNewingEventArgs e)
        {
            if (Newing != null)
            {
                Newing(null, e);
            }
        }

        /// <summary>
        /// Occurs when [before delete].
        /// </summary>
        public new static event DeleteEventHandler BeforeDelete;

        /// <summary>
        /// Raises the <see cref="E:BeforeDelete"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected new virtual void FireBeforeDelete(DeleteEventArgs e)
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
        protected new virtual void FireAfterDelete(DeleteEventArgs e)
        {
            if (AfterDelete != null)
                AfterDelete(this, e);
        }


        /// <summary>
        /// Occurs when [before delete].
        /// </summary>
        public static event MoveToTrashEventHandler BeforeMoveToTrash;
        /// <summary>
        /// Raises the <see cref="E:BeforeDelete"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void FireBeforeMoveToTrash(MoveToTrashEventArgs e)
        {
            if (BeforeMoveToTrash != null)
                BeforeMoveToTrash(this, e);
        }


        /// <summary>
        /// Occurs when [after move to trash].
        /// </summary>
        public static event MoveToTrashEventHandler AfterMoveToTrash;
        /// <summary>
        /// Fires the after move to trash.
        /// </summary>
        /// <param name="e">The <see cref="umbraco.cms.businesslogic.MoveToTrashEventArgs"/> instance containing the event data.</param>
        protected virtual void FireAfterMoveToTrash(MoveToTrashEventArgs e)
        {
            if (AfterMoveToTrash != null)
                AfterMoveToTrash(this, e);
        }

        /// <summary>
        /// Occurs when [before publish].
        /// </summary>
        public static event PublishEventHandler BeforePublish;
        /// <summary>
        /// Raises the <see cref="E:BeforePublish"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void FireBeforePublish(PublishEventArgs e)
        {
            if (BeforePublish != null)
                BeforePublish(this, e);
        }

        /// <summary>
        /// Occurs when [after publish].
        /// </summary>
        public static event PublishEventHandler AfterPublish;
        /// <summary>
        /// Raises the <see cref="E:AfterPublish"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void FireAfterPublish(PublishEventArgs e)
        {
            if (AfterPublish != null)
                AfterPublish(this, e);
        }
        /// <summary>
        /// Occurs when [before publish].
        /// </summary>
        public static event SendToPublishEventHandler BeforeSendToPublish;
        /// <summary>
        /// Raises the <see cref="E:BeforePublish"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void FireBeforeSendToPublish(SendToPublishEventArgs e)
        {
            if (BeforeSendToPublish != null)
                BeforeSendToPublish(this, e);
        }


        /// <summary>
        /// Occurs when [after publish].
        /// </summary>
        public static event SendToPublishEventHandler AfterSendToPublish;
        /// <summary>
        /// Raises the <see cref="E:AfterPublish"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void FireAfterSendToPublish(SendToPublishEventArgs e)
        {
            if (AfterSendToPublish != null)
                AfterSendToPublish(this, e);
        }

        /// <summary>
        /// Occurs when [before un publish].
        /// </summary>
        public static event UnPublishEventHandler BeforeUnPublish;
        /// <summary>
        /// Raises the <see cref="E:BeforeUnPublish"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void FireBeforeUnPublish(UnPublishEventArgs e)
        {
            if (BeforeUnPublish != null)
                BeforeUnPublish(this, e);
        }

        /// <summary>
        /// Occurs when [after un publish].
        /// </summary>
        public static event UnPublishEventHandler AfterUnPublish;
        /// <summary>
        /// Raises the <see cref="E:AfterUnPublish"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void FireAfterUnPublish(UnPublishEventArgs e)
        {
            if (AfterUnPublish != null)
                AfterUnPublish(this, e);
        }

        /// <summary>
        /// Occurs when [before copy].
        /// </summary>
        public static event CopyEventHandler BeforeCopy;
        /// <summary>
        /// Raises the <see cref="E:BeforeCopy"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void FireBeforeCopy(CopyEventArgs e)
        {
            if (BeforeCopy != null)
                BeforeCopy(this, e);
        }

        /// <summary>
        /// Occurs when [after copy].
        /// </summary>
        public static event CopyEventHandler AfterCopy;
        /// <summary>
        /// Raises the <see cref="E:AfterCopy"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void FireAfterCopy(CopyEventArgs e)
        {
            if (AfterCopy != null)
                AfterCopy(this, e);
        }

        /// <summary>
        /// Occurs when [before roll back].
        /// </summary>
        public static event RollBackEventHandler BeforeRollBack;
        /// <summary>
        /// Raises the <see cref="E:BeforeRollBack"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void FireBeforeRollBack(RollBackEventArgs e)
        {
            if (BeforeRollBack != null)
                BeforeRollBack(this, e);
        }

        /// <summary>
        /// Occurs when [after roll back].
        /// </summary>
        public static event RollBackEventHandler AfterRollBack;
        /// <summary>
        /// Raises the <see cref="E:AfterRollBack"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void FireAfterRollBack(RollBackEventArgs e)
        {
            if (AfterRollBack != null)
                AfterRollBack(this, e);
        }
        #endregion


    }
}
