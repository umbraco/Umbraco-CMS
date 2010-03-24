using System;
using System.Collections;
using System.Data;
using System.Web;
using System.Xml;

using umbraco.BusinessLogic;
using umbraco.BusinessLogic.Actions;
using umbraco.BusinessLogic.console;

using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.relation;
using umbraco.cms.helpers;
using umbraco.DataLayer;
using System.Collections.Generic;
using umbraco.cms.businesslogic.propertytype;
using System.Linq;
using System.ComponentModel;
using umbraco.IO;

namespace umbraco.cms.businesslogic.web
{
    /// <summary>
    /// Document represents a webpage,
    /// type (umbraco.cms.businesslogic.web.DocumentType)
    /// 
    /// Pubished Documents are exposed to the runtime/the public website in a cached xml document.
    /// </summary>
    public class Document : Content
    {
        private const string m_SQLOptimizedSingle = @"
                Select 
	                (select count(id) from umbracoNode where parentId = @id) as Children, 
	                (select Count(published) as tmp from cmsDocument where published = 1 And nodeId = @id) as Published,
	                cmsContentVersion.VersionId,
                    cmsContentVersion.versionDate,	                
	                contentTypeNode.uniqueId as ContentTypeGuid, 
					cmsContent.ContentType, cmsContentType.icon, cmsContentType.alias, cmsContentType.thumbnail, cmsContentType.description, cmsContentType.masterContentType, cmsContentType.nodeId as contentTypeId
	                published, documentUser, coalesce(templateId, cmsDocumentType.templateNodeId) as templateId, cmsDocument.text as DocumentText, releaseDate, expireDate, updateDate, 
	                umbracoNode.createDate, umbracoNode.trashed, umbracoNode.parentId, umbracoNode.nodeObjectType, umbracoNode.nodeUser, umbracoNode.level, umbracoNode.path, umbracoNode.sortOrder, umbracoNode.uniqueId, umbracoNode.text 
                from 
	                umbracoNode 
                inner join
	                cmsContentVersion on cmsContentVersion.contentID = umbracoNode.id
                inner join 
	                cmsDocument on cmsDocument.versionId = cmsContentVersion.versionId
                inner join
	                cmsContent on cmsDocument.nodeId = cmsContent.NodeId
                inner join
	                cmsContentType on cmsContentType.nodeId = cmsContent.ContentType
                inner join 
	                umbracoNode contentTypeNode on contentTypeNode.id = cmsContentType.nodeId
                left join cmsDocumentType on 
	                cmsDocumentType.contentTypeNodeId = cmsContent.contentType and cmsDocumentType.IsDefault = 1 
                where 
	                {0}
                order by
	                {1}
                ";
        private const string m_SQLOptimizedChildren = @"
                select count(children.id) as children, umbracoNode.id, umbracoNode.uniqueId, umbracoNode.level, umbracoNode.parentId, cmsDocument.documentUser,
                    coalesce(cmsDocument.templateId, cmsDocumentType.templateNodeId) as templateId, umbracoNode.path, umbracoNode.sortOrder, coalesce(publishCheck.published,0) as published, umbracoNode.createDate, cmsDocument.text, cmsDocument.updateDate, cmsContentVersion.versionDate, cmsContentType.icon, cmsContentType.alias, cmsContentType.thumbnail, cmsContentType.description, cmsContentType.masterContentType, cmsContentType.nodeId as contentTypeId
                from umbracoNode
                left join umbracoNode children on children.parentId = umbracoNode.id
                inner join cmsContent on cmsContent.nodeId = umbracoNode.id
                inner join cmsContentType on cmsContentType.nodeId = cmsContent.contentType
                inner join (select contentId, max(versionDate) AS versionDate from cmsContentVersion
                            inner join umbracoNode on umbracoNode.id = cmsContentVersion.contentId and umbracoNode.parentId = @parentId
                            group by contentId) AS temp
                on temp.contentId = cmsContent.nodeId
                inner join cmsContentVersion on cmsContentVersion.contentId = temp.contentId and cmsContentVersion.versionDate = temp.versionDate
                inner join cmsDocument on cmsDocument.versionId = cmsContentversion.versionId
                left join cmsDocument publishCheck on publishCheck.nodeId = cmsContent.nodeID and publishCheck.published = 1
                left join cmsDocumentType on
                    cmsDocumentType.contentTypeNodeId = cmsContent.contentType and cmsDocumentType.IsDefault = 1
                where {0}
                group by umbracoNode.id, umbracoNode.uniqueId, umbracoNode.level, umbracoNode.parentId, cmsDocument.documentUser, cmsDocument.templateId, cmsDocumentType.templateNodeId, umbracoNode.path, umbracoNode.sortOrder, coalesce(publishCheck.published,0), umbracoNode.createDate, cmsDocument.text, cmsDocument.updateDate, cmsContentVersion.versionDate, cmsContentType.icon, cmsContentType.alias, cmsContentType.thumbnail, cmsContentType.description, cmsContentType.masterContentType, cmsContentType.nodeId
                order by {1}
                ";

        private const string m_SQLOptimizedForPreview = @"
                select umbracoNode.id, umbracoNode.parentId, umbracoNode.level, umbracoNode.sortOrder, cmsDocument.versionId, cmsPreviewXml.xml from cmsDocument
                inner join umbracoNode on umbracoNode.id = cmsDocument.nodeId
                inner join cmsPreviewXml on cmsPreviewXml.nodeId = cmsDocument.nodeId and cmsPreviewXml.versionId = cmsDocument.versionId
                where newest = 1 and trashed = 0 and path like '{0}'
                order by level,sortOrder
 ";

        public static Guid _objectType = new Guid("c66ba18e-eaf3-4cff-8a22-41b16d66a972");
        private DateTime _updated;
        private DateTime _release;
        private DateTime _expire;
        private int _template;
        private string _text;
        private bool _published;
        private XmlNode _xml;
        private User _creator;
        private User _writer;
        private bool _optimizedMode;

        /// <summary>
        /// This is used to cache the child documents of Document when the children property
        /// is accessed or enumerated over, this will save alot of database calls.
        /// </summary>
        private IEnumerable<Document> _children = null;

        // special for passing httpcontext object
        private HttpContext _httpContext;

        // special for tree performance
        private int _userId = -1;

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
            get { return _creator; }
        }

        /// <summary>
        /// Gets the writer.
        /// </summary>
        /// <value>The writer.</value>
        public User Writer
        {
            get { return _writer; }
        }

        /// <summary>
        /// The current HTTPContext
        /// </summary>
        public HttpContext HttpContext
        {
            set { _httpContext = value; }
            get
            {
                if (_httpContext == null)
                    _httpContext = HttpContext.Current;
                return _httpContext;
            }
        }

        /// <summary>
        /// Executes handlers and events for the Send To Publication action.
        /// </summary>
        /// <param name="u">The User</param>
        public bool SendToPublication(User u)
        {
            SendToPublishEventArgs e = new SendToPublishEventArgs();
            FireBeforeSendToPublish(e);
            if (!e.Cancel)
            {
                BusinessLogic.Log.Add(BusinessLogic.LogTypes.SendToPublish, u, this.Id, "");

                BusinessLogic.Actions.Action.RunActionHandlers(this, ActionToPublish.Instance);

                FireAfterSendToPublish(e);
                return true;
            }

            return false;

        }

        /// <summary>
        /// Publishing a document
        /// A xmlrepresentation of the document and its data are exposed to the runtime data
        /// (an xmlrepresentation is added -or updated if the document previously are published) ,
        /// this will lead to a new version of the document being created, for continuing editing of
        /// the data.
        /// </summary>
        /// <param name="u">The usercontext under which the action are performed</param>
        public void Publish(User u)
        {
            PublishWithResult(u);
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
        public bool PublishWithResult(User u)
        {
            PublishEventArgs e = new PublishEventArgs();
            FireBeforePublish(e);

            if (!e.Cancel)
            {

                // make a lookup to see if template is 0 as the template is not initialized in the optimized
                // Document.Children method which is used in PublishWithChildrenWithResult methhod
                if (_template == 0)
                {
                    _template = new DocumentType(this.ContentType.Id).DefaultTemplate;
                }

                _published = true;
                string tempVersion = Version.ToString();
                Guid newVersion = createNewVersion();

                Log.Add(LogTypes.Publish, u, Id, "");

                //PPH make sure that there is only 1 newest node, this is important in regard to schedueled publishing...
                SqlHelper.ExecuteNonQuery("update cmsDocument set newest = 0 where nodeId = " + Id);

                SqlHelper.ExecuteNonQuery("insert into cmsDocument (newest, nodeId, published, documentUser, versionId, Text, TemplateId) values (1," +
                                          Id + ", 0, " + u.Id + ", @versionId, @text,"
                                          + _template + ")",
                                          SqlHelper.CreateParameter("@versionId", newVersion),
                                          SqlHelper.CreateParameter("@text", Text));
                SqlHelper.ExecuteNonQuery("update cmsDocument set published = 0 where nodeId = " + Id);
                SqlHelper.ExecuteNonQuery("update cmsDocument set published = 1, newest = 0 where versionId = @versionId",
                                            SqlHelper.CreateParameter("@versionId", tempVersion));

                // update release and expire dates
                Document newDoc = new Document(Id, newVersion);
                if (ReleaseDate != new DateTime())
                    newDoc.ReleaseDate = ReleaseDate;
                if (ExpireDate != new DateTime())
                    newDoc.ExpireDate = ExpireDate;

                // Update xml in db using the new document (has correct version date)
                newDoc.XmlGenerate(new XmlDocument());

                FireAfterPublish(e);

                return true;
            }
            else
            {
                return false;
            }
        }


        public bool PublishWithChildrenWithResult(User u)
        {
            if (PublishWithResult(u))
            {
                //store children array here because iterating over an Array object is very inneficient.
                var c = this.Children;
                foreach (cms.businesslogic.web.Document dc in c)
                {
                    dc.PublishWithChildrenWithResult(u);
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Rollbacks a document to a previous version, this will create a new version of the document and copy
        /// all of the old documents data.
        /// </summary>
        /// <param name="u">The usercontext under which the action are performed</param>
        /// <param name="VersionId">The unique Id of the version to roll back to</param>
        public void RollBack(Guid VersionId, User u)
        {
            RollBackEventArgs e = new RollBackEventArgs();
            FireBeforeRollBack(e);

            if (!e.Cancel)
            {
                Guid newVersion = createNewVersion();
                SqlHelper.ExecuteNonQuery("insert into cmsDocument (nodeId, published, documentUser, versionId, Text, TemplateId) values (" +
                                          Id +
                                          ", 0, " + u.Id + ", @versionId, @text, " +
                                          _template + ")",
                                          SqlHelper.CreateParameter("@versionId", newVersion),
                                          SqlHelper.CreateParameter("@text", Text));

                // Get new version
                Document dNew = new Document(Id, newVersion);

                // Old version
                Document dOld = new Document(Id, VersionId);

                // Revert title
                dNew.Text = dOld.Text;

                // Revert all properties
                var props = dOld.getProperties;
                foreach (Property p in props)
                    try
                    {
                        dNew.getProperty(p.PropertyType).Value = p.Value;
                    }
                    catch
                    {
                        // property doesn't exists
                    }

                FireAfterRollBack(e);
            }
        }

        /// <summary>
        /// Recursive publishing.
        /// 
        /// Envoking this method will publish the documents and all children recursive.
        /// </summary>
        /// <param name="u">The usercontext under which the action are performed</param>
        public void PublishWithSubs(User u)
        {

            PublishEventArgs e = new PublishEventArgs();
            FireBeforePublish(e);

            if (!e.Cancel)
            {
                _published = true;
                string tempVersion = Version.ToString();
                Guid newVersion = createNewVersion();

                SqlHelper.ExecuteNonQuery("insert into cmsDocument (nodeId, published, documentUser, versionId, Text) values (" +
                                          Id + ", 0, " + u.Id +
                                          ", @versionId, @text)",
                                          SqlHelper.CreateParameter("@versionId", newVersion),
                                          SqlHelper.CreateParameter("@text", Text));

                SqlHelper.ExecuteNonQuery("update cmsDocument set published = 0 where nodeId = " + Id);
                SqlHelper.ExecuteNonQuery("update cmsDocument set published = 1 where versionId = @versionId", SqlHelper.CreateParameter("@versionId", tempVersion));

                BusinessLogic.Log.Add(LogTypes.Debug, -1, newVersion.ToString() + " - " + Id.ToString());

                // Update xml in db
                XmlGenerate(new XmlDocument());

                //store children array here because iterating over an Array object is very inneficient.
                var c = Children;
                foreach (Document dc in c)
                    dc.PublishWithSubs(u);

                FireAfterPublish(e);
            }
        }

        /// <summary>
        /// Published flag is on if the document are published
        /// </summary>
        public bool Published
        {
            get { return _published; }

            set
            {
                // Esben Carlsen: ????? value never used ?? --> needs update
                _published = false;
                SqlHelper.ExecuteNonQuery(
                                          string.Format("update cmsDocument set published = 0 where nodeId = {0}", Id));
            }
        }

        public void UnPublish()
        {
            UnPublishEventArgs e = new UnPublishEventArgs();

            FireBeforeUnPublish(e);

            if (!e.Cancel)
            {
                SqlHelper.ExecuteNonQuery(string.Format("update cmsDocument set published = 0 where nodeId = {0}", Id));
                FireAfterUnPublish(e);
            }
        }

        /// <summary>
        /// Constructs a new document
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <param name="noSetup">true if the data shouldn't loaded from the db</param>
        public Document(Guid id, bool noSetup)
            : base(id)
        {

        }

        /// <summary>
        /// Initializes a new instance of the Document class.
        /// You can set an optional flag noSetup, used for optimizing for loading nodes in the tree, 
        /// therefor only data needed by the tree is initialized.
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <param name="noSetup">true if the data shouldn't loaded from the db</param>
        public Document(int id, bool noSetup)
            : base(id, noSetup)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Document class to a specific version, used for rolling back data from a previous version
        /// of the document.
        /// </summary>
        /// <param name="id">The id of the document</param>
        /// <param name="Version">The version of the document</param>
        public Document(int id, Guid Version)
            : base(id)
        {
            this.Version = Version;
            setupDocument();
        }

        /// <summary>
        /// Initializes a new instance of the Document class.
        /// </summary>
        /// <param name="id">The id of the document</param>
        public Document(int id)
            : base(id)
        {
            setupDocument();
        }

        /// <summary>
        /// Initialize the document
        /// </summary>
        /// <param name="id">The id of the document</param>
        public Document(Guid id)
            : base(id)
        {
            setupDocument();
        }

        //TODO: SD: Implement this EVERYWHERE (90 places apparently)
        public Document(bool optimizedMode, int id)
            : base(id, optimizedMode)
        {
            this._optimizedMode = OptimizedMode;

            if (OptimizedMode)
            {

                using (IRecordsReader dr =
                        SqlHelper.ExecuteReader(string.Format(m_SQLOptimizedSingle, "umbracoNode.id = @id", "cmsContentVersion.id desc"),
                    SqlHelper.CreateParameter("@id", id)))
                {
                    if (dr.Read())
                    {
                        // Initialize node and basic document properties
                        bool _hc = false;
                        if (dr.GetInt("children") > 0)
                            _hc = true;
                        int? masterContentType = null;
                        if (!dr.IsNull("masterContentType"))
                            masterContentType = dr.GetInt("masterContentType");
                        SetupDocumentForTree(dr.GetGuid("uniqueId")
                            , dr.GetShort("level")
                            , dr.GetInt("parentId")
                            , dr.GetInt("documentUser")
                            , dr.GetBoolean("published")
                            , dr.GetString("path")
                            , dr.GetString("text")
                            , dr.GetDateTime("createDate")
                            , dr.GetDateTime("updateDate")
                            , dr.GetDateTime("versionDate")
                            , dr.GetString("icon")
                            , _hc
                            , dr.GetString("alias")
                            , dr.GetString("thumbnail")
                            , dr.GetString("description")
                            , masterContentType
                            , dr.GetInt("contentTypeId")
                            , dr.GetInt("templateId")
                            );

                        // initialize content object
                        InitializeContent(dr.GetInt("ContentType"), dr.GetGuid("versionId"),
                                          dr.GetDateTime("versionDate"), dr.GetString("icon"));

                        // initialize final document properties
                        DateTime tmpReleaseDate = new DateTime();
                        DateTime tmpExpireDate = new DateTime();
                        if (!dr.IsNull("releaseDate"))
                            tmpReleaseDate = dr.GetDateTime("releaseDate");
                        if (!dr.IsNull("expireDate"))
                            tmpExpireDate = dr.GetDateTime("expireDate");

                        InitializeDocument(
                            new User(dr.GetInt("nodeUser"), true),
                            new User(dr.GetInt("documentUser"), true),
                            dr.GetString("documentText"),
                            dr.GetInt("templateId"),
                            tmpReleaseDate,
                            tmpExpireDate,
                            dr.GetDateTime("updateDate"),
                            dr.GetBoolean("published")
                            );
                    }
                }
            }
        }

        public Document()
        {

        }

        /// <summary>
        /// Used to persist object changes to the database. In Version3.0 it's just a stub for future compatibility
        /// </summary>
        public override void Save()
        {
            SaveEventArgs e = new SaveEventArgs();
            FireBeforeSave(e);

            if (!e.Cancel)
            {

                if (this._optimizedMode)
                {
                    foreach (var property in this._knownProperties)
                    {
                        var pt = property.Key;
                        pt.Value = property.Value;
                    }
                }

                base.Save();
                // update preview xml
                SaveXmlPreview(new XmlDocument());

                FireAfterSave(e);
            }
        }

        //TODO: Perhaps this should override the setupNode method of the CMSNode so that constructors work properly!
        private void setupDocument()
        {
            IRecordsReader dr =
                SqlHelper.ExecuteReader("select published, documentUser, coalesce(templateId, cmsDocumentType.templateNodeId) as templateId, text, releaseDate, expireDate, updateDate from cmsDocument inner join cmsContent on cmsDocument.nodeId = cmsContent.Nodeid left join cmsDocumentType on cmsDocumentType.contentTypeNodeId = cmsContent.contentType and cmsDocumentType.IsDefault = 1 where versionId = @versionId",
                                        SqlHelper.CreateParameter("@versionId", Version));
            if (dr.Read())
            {
                _creator = User;
                _writer = User.GetUser(dr.GetInt("documentUser"));

                _text = dr.GetString("text");
                if (!dr.IsNull("templateId"))
                    _template = dr.GetInt("templateId");
                if (!dr.IsNull("releaseDate"))
                    _release = dr.GetDateTime("releaseDate");
                if (!dr.IsNull("expireDate"))
                    _expire = dr.GetDateTime("expireDate");
                if (!dr.IsNull("updateDate"))
                    _updated = dr.GetDateTime("updateDate");
            }
            dr.Close();
            _published = (SqlHelper.ExecuteScalar<int>("select Count(published) as tmp from cmsDocument where published = 1 And nodeId =" + Id) > 0);
        }

        protected void InitializeDocument(User InitUser, User InitWriter, string InitText, int InitTemplate,
                                          DateTime InitReleaseDate, DateTime InitExpireDate, DateTime InitUpdateDate,
                                          bool InitPublished)
        {
            _creator = InitUser;
            _writer = InitWriter;
            _text = InitText;
            _template = InitTemplate;
            _release = InitReleaseDate;
            _expire = InitExpireDate;
            _updated = InitUpdateDate;
            _published = InitPublished;
        }

        /// <summary>
        /// The name of the document, amongst other used in the nice url.
        /// </summary>
        public new string Text
        {
            get
            {
                if (_text == null || _text == "")
                    _text = SqlHelper.ExecuteScalar<string>(string.Format("select text from umbracoNode where id = {0}", Id));
                return _text;
            }
            set
            {
                _text = value;
                base.Text = value;
                SqlHelper.ExecuteNonQuery("update cmsDocument set text = @text where versionId = @versionId",
                                          SqlHelper.CreateParameter("@text", _text),
                                          SqlHelper.CreateParameter("@versionId", Version));
                CMSNode c = new CMSNode(Id);
                c.Text = _text;

            }
        }

        /// <summary>
        /// The date of the last update of the document
        /// </summary>
        public DateTime UpdateDate
        {
            get { return _updated; }
            set
            {
                _updated = value;
                SqlHelper.ExecuteNonQuery("update cmsDocument set updateDate = @value where versionId = @versionId",
                                          SqlHelper.CreateParameter("@value", value),
                                          SqlHelper.CreateParameter("@versionId", Version));
            }
        }

        /// <summary>
        /// A datestamp which indicates when a document should be published, used in automated publish/unpublish scenarios
        /// </summary>
        public DateTime ReleaseDate
        {
            get { return _release; }
            set
            {
                _release = value;

                if (_release.Year != 1 || _release.Month != 1 || _release.Day != 1)
                    SqlHelper.ExecuteNonQuery("update cmsDocument set releaseDate = @value where versionId = @versionId",
                                              SqlHelper.CreateParameter("@value", value),
                                              SqlHelper.CreateParameter("@versionId", Version));
                else
                    SqlHelper.ExecuteNonQuery("update cmsDocument set releaseDate = NULL where versionId = @versionId",
                                              SqlHelper.CreateParameter("@versionId", Version));
            }
        }

        /// <summary>
        /// A datestamp which indicates when a document should be unpublished, used in automated publish/unpublish scenarios
        /// </summary>
        public DateTime ExpireDate
        {
            get { return _expire; }
            set
            {
                _expire = value;

                if (_expire.Year != 1 || _expire.Month != 1 || _expire.Day != 1)
                    SqlHelper.ExecuteNonQuery("update cmsDocument set expireDate = @value where versionId=@versionId",
                                              SqlHelper.CreateParameter("@value", value),
                                              SqlHelper.CreateParameter("@versionId", Version));
                else
                    SqlHelper.ExecuteNonQuery("update cmsDocument set expireDate = NULL where versionId=@versionId",
                                              SqlHelper.CreateParameter("@versionId", Version));
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
        public int Template
        {
            get { return _template; }
            set
            {
                _template = value;
                SqlHelper.ExecuteNonQuery("update cmsDocument set templateId = @value where versionId = @versionId",
                                          SqlHelper.CreateParameter("@value", _template),
                                          SqlHelper.CreateParameter("@versionId", Version));
            }
        }

        /// <summary>
        /// Used for rolling back documents to a previous version
        /// </summary>
        /// <returns> Previous published versions of the document</returns>
        public DocumentVersionList[] GetVersions()
        {
            ArrayList versions = new ArrayList();
            using (IRecordsReader dr =
                SqlHelper.ExecuteReader("select documentUser, versionId, updateDate, text from cmsDocument where nodeId = @nodeId order by updateDate",
                                        SqlHelper.CreateParameter("@nodeId", Id)))
            {
                while (dr.Read())
                {
                    DocumentVersionList dv =
                        new DocumentVersionList(dr.GetGuid("versionId"),
                                                dr.GetDateTime("updateDate"),
                                                dr.GetString("text"),
                                                User.GetUser(dr.GetInt("documentUser")));
                    versions.Add(dv);
                }
            }

            DocumentVersionList[] retVal = new DocumentVersionList[versions.Count];
            int i = 0;
            foreach (DocumentVersionList dv in versions)
            {
                retVal[i] = dv;
                i++;
            }
            return retVal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Returns a breadcrumlike path for the document like: /ancestorname/ancestorname</returns>
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
        /// Creates a new document of the same type and copies all data from the current onto it
        /// </summary>
        /// <param name="CopyTo">The parentid where the document should be copied to</param>
        /// <param name="u">The usercontext under which the action are performed</param>
        public void Copy(int CopyTo, User u)
        {
            Copy(CopyTo, u, false);
        }

        public void Copy(int CopyTo, User u, bool RelateToOrignal)
        {
            CopyEventArgs e = new CopyEventArgs();

            FireBeforeCopy(e);

            if (!e.Cancel)
            {
                // Make the new document
                Document NewDoc = MakeNew(Text, new DocumentType(ContentType.Id), u, CopyTo);

                // update template
                NewDoc.Template = Template;

                // Copy the properties of the current document
                var props = getProperties;
                foreach (Property p in props)
                    NewDoc.getProperty(p.PropertyType.Alias).Value = p.Value;

                // Relate?
                if (RelateToOrignal)
                {
                    Relation.MakeNew(Id, NewDoc.Id, RelationType.GetByAlias("relateDocumentOnCopy"), "");

                    // Add to audit trail
                    Log.Add(LogTypes.Copy, u, NewDoc.Id, "Copied and related from " + Text + " (id: " + Id.ToString() + ")");
                }


                // Copy the children
                //store children array here because iterating over an Array object is very inneficient.
                var c = Children;
                foreach (Document d in c)
                    d.Copy(NewDoc.Id, u, RelateToOrignal);


                FireAfterCopy(e);
            }
        }

        /// <summary>
        /// Creates a new document
        /// </summary>
        /// <param name="Name">The name (.Text property) of the document</param>
        /// <param name="dct">The documenttype</param>
        /// <param name="u">The usercontext under which the action are performed</param>
        /// <param name="ParentId">The id of the parent to the document</param>
        /// <returns>The newly created document</returns>
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


            Guid newId = Guid.NewGuid();

            // Updated to match level from base node
            CMSNode n = new CMSNode(ParentId);
            int newLevel = n.Level;
            newLevel++;
            MakeNew(ParentId, _objectType, u.Id, newLevel, Name, newId);
            Document tmp = new Document(newId, true);
            tmp.CreateContent(dct);
            SqlHelper.ExecuteNonQuery("insert into cmsDocument (newest, nodeId, published, documentUser, versionId, Text) values (1, " +
                                      tmp.Id + ", 0, " +
                                      u.Id + ", @versionId, @text)",
                                      SqlHelper.CreateParameter("@versionId", tmp.Version),
                                      SqlHelper.CreateParameter("@text", tmp.Text));

            // Update the sortOrder if the parent was the root!
            if (ParentId == -1)
            {
                CMSNode c = new CMSNode(newId);
                c.sortOrder = GetRootDocuments().Length + 1;
            }

            Document d = new Document(newId);

            //event
            NewEventArgs e = new NewEventArgs();
            d.OnNew(e);

            // Log
            Log.Add(LogTypes.New, u, d.Id, "");

            // Run Handler				
            umbraco.BusinessLogic.Actions.Action.RunActionHandlers(d, ActionNew.Instance);

            // Save doc
            d.Save();

            return d;
        }


        /// <summary>
        /// Used to get the firstlevel/root documents of the hierachy
        /// </summary>
        /// <returns>Root documents</returns>
        public static Document[] GetRootDocuments()
        {
            Guid[] topNodeIds = TopMostNodeIds(_objectType);

            Document[] retval = new Document[topNodeIds.Length];
            for (int i = 0; i < topNodeIds.Length; i++)
            {
                Document d = new Document(topNodeIds[i]);
                retval[i] = d;
            }
            return retval;
        }

        /// <summary>
        /// A collection of documents imidiately underneath this document ie. the childdocuments
        /// </summary>
        public new Document[] Children
        {
            get
            {
                //SD: Removed old, non-optimized method!
                //IconI[] tmp = base.Children;
                //Document[] retval = new Document[tmp.Length];
                //for (int i = 0; i < tmp.Length; i++) retval[i] = new Document(tmp[i].Id);
                //return retval;

                //cache the documents children so that this db call doesn't have to occur again
                if (this._children == null)
                    this._children = Document.GetChildrenForTree(this.Id);

                return this._children.ToArray();
            }
        }

        public static int CountSubs(int parentId, bool publishedOnly)
        {
            if (!publishedOnly)
            {
                return CountSubs(parentId);
            }
            else
            {
                return SqlHelper.ExecuteScalar<int>("SELECT COUNT(distinct nodeId) FROM umbracoNode INNER JOIN cmsDocument ON cmsDocument.published = 1 and cmsDocument.nodeId = umbracoNode.id WHERE ','+path+',' LIKE '%," + parentId.ToString() + ",%'");
            }
        }

        /// <summary>
        /// Deletes the current document (and all children recursive)
        /// </summary>
        public new void delete()
        {
            // Check for recyle bin
            if (!Path.Contains("," + ((int)RecycleBin.RecycleBinType.Content).ToString() + ","))
            {
                MoveToTrashEventArgs e = new MoveToTrashEventArgs();
                FireBeforeMoveToTrash(e);

                if (!e.Cancel)
                {
                    umbraco.BusinessLogic.Actions.Action.RunActionHandlers(this, ActionDelete.Instance);
                    UnPublish();
                    Move(-20);

                    FireAfterMoveToTrash(e);
                }

            }
            else
            {

                DeleteEventArgs e = new DeleteEventArgs();

                FireBeforeDelete(e);

                if (!e.Cancel)
                {

                    //store children array here because iterating over an Array object is very inneficient.
                    var c = Children;
                    foreach (Document d in c)
                    {
                        d.delete();
                    }

                    umbraco.BusinessLogic.Actions.Action.RunActionHandlers(this, ActionDelete.Instance);

                    //delete files 
                    interfaces.IDataType uploadField = new cms.businesslogic.datatype.controls.Factory().GetNewObject(new Guid("5032a6e6-69e3-491d-bb28-cd31cd11086c"));
                    var props = this.getProperties;
                    foreach (cms.businesslogic.property.Property p in props)

                        if (p.PropertyType.DataTypeDefinition.DataType.Id == uploadField.Id &&
                            p.Value.ToString() != "" &&
                            System.IO.File.Exists(IOHelper.MapPath(p.Value.ToString()))
                            )
                            System.IO.File.Delete(IOHelper.MapPath(p.Value.ToString()));


                    SqlHelper.ExecuteNonQuery("delete from cmsDocument where NodeId = " + Id);
                    HttpContext.Current.Trace.Write("documentdelete", "base delete");
                    base.delete();
                    HttpContext.Current.Trace.Write("documentdelete", "after base delete");

                    FireAfterDelete(e);
                }
            }
        }

        /// <summary>
        /// Deletes all documents of a type, will be invoked if a documenttype is deleted.
        /// 
        /// Note: use with care: this method can result in wast amount of data being deleted.
        /// </summary>
        /// <param name="dt">The type of which documents should be deleted</param>
        public static void DeleteFromType(DocumentType dt)
        {
            var objs = getContentOfContentType(dt);
            foreach (Content c in objs)
            {
                // due to recursive structure document might already been deleted..
                if (IsNode(c.UniqueId))
                {
                    Document tmp = new Document(c.UniqueId);
                    tmp.delete();
                }
            }
        }

        /// <summary>
        /// Refreshes the xml, used when publishing data on a document which already is published
        /// </summary>
        /// <param name="xd">The source xmldocument</param>
        /// <param name="x">The previous xmlrepresentation of the document</param>
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
        public new void XmlGenerate(XmlDocument xd)
        {
            XmlNode x = generateXmlWithoutSaving(xd);
            /*
                        if (!UmbracoSettings.UseFriendlyXmlSchema)
                        {
                        } else
                        {
                            XmlNode childNodes = xmlHelper.addTextNode(xd, "data", "");
                            x.AppendChild(childNodes);
                            XmlPopulate(xd, ref childNodes, false);
                        }
            */


            // Save to db
            saveXml(x);
        }


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
        /// A xmlrepresentaion of the document, used when publishing/exporting the document, 
        /// 
        /// Optional: Recursive get childdocuments xmlrepresentation
        /// </summary>
        /// <param name="xd">The xmldocument</param>
        /// <param name="Deep">Recursive add of childdocuments</param>
        /// <returns></returns>
        public new virtual XmlNode ToXml(XmlDocument xd, bool Deep)
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
                    //store children array here because iterating over an Array property object is very inneficient.
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
            string urlName = this.Text;
            var props = getProperties;
            foreach (Property p in props)
                if (p != null)
                {
                    x.AppendChild(p.ToXml(xd));
                    if (p.PropertyType.Alias == "umbracoUrlName" && p.Value.ToString().Trim() != string.Empty)
                        urlName = p.Value.ToString();
                }

            // attributes
            x.Attributes.Append(addAttribute(xd, "id", Id.ToString()));
            //            x.Attributes.Append(addAttribute(xd, "version", Version.ToString()));
            if (Level > 1)
                x.Attributes.Append(addAttribute(xd, "parentID", Parent.Id.ToString()));
            else
                x.Attributes.Append(addAttribute(xd, "parentID", "-1"));
            x.Attributes.Append(addAttribute(xd, "level", Level.ToString()));
            x.Attributes.Append(addAttribute(xd, "writerID", _writer.Id.ToString()));
            x.Attributes.Append(addAttribute(xd, "creatorID", _creator.Id.ToString()));
            if (ContentType != null)
                x.Attributes.Append(addAttribute(xd, "nodeType", ContentType.Id.ToString()));
            x.Attributes.Append(addAttribute(xd, "template", _template.ToString()));
            x.Attributes.Append(addAttribute(xd, "sortOrder", sortOrder.ToString()));
            x.Attributes.Append(addAttribute(xd, "createDate", CreateDateTime.ToString("s")));
            x.Attributes.Append(addAttribute(xd, "updateDate", VersionDate.ToString("s")));
            x.Attributes.Append(addAttribute(xd, "nodeName", Text));
            x.Attributes.Append(addAttribute(xd, "urlName", url.FormatUrl(urlName.ToLower())));
            x.Attributes.Append(addAttribute(xd, "writerName", _writer.Name));
            x.Attributes.Append(addAttribute(xd, "creatorName", _creator.Name.ToString()));
            if (ContentType != null && UmbracoSettings.UseLegacyXmlSchema)
                x.Attributes.Append(addAttribute(xd, "nodeTypeAlias", ContentType.Alias));
            x.Attributes.Append(addAttribute(xd, "path", Path));

            if (!UmbracoSettings.UseLegacyXmlSchema)
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
                        Log.Add(LogTypes.System, d.Id, "Document not published so XML cannot be generated");
                    }
                }

            }
        }

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

        #region XmlPreivew

        public override XmlNode ToPreviewXml(XmlDocument xd)
        {
            if (!PreviewExists(Version))
            {
                SaveXmlPreview(xd);
            }
            return GetPreviewXml(xd, Version);
        }

        protected void SaveXmlPreview(XmlDocument xd)
        {
            savePreviewXml(generateXmlWithoutSaving(xd), Version);
        }

        #endregion

        private XmlAttribute addAttribute(XmlDocument Xd, string Name, string Value)
        {
            XmlAttribute temp = Xd.CreateAttribute(Name);
            temp.Value = Value;
            return temp;
        }

        /// <summary>
        /// Performance tuned method for use in the tree
        /// </summary>
        /// <param name="NodeId">The parentdocuments id</param>
        /// <returns></returns>
        public static Document[] GetChildrenForTree(int NodeId)
        {
            ArrayList tmp = new ArrayList();
            using (IRecordsReader dr =
                SqlHelper.ExecuteReader(
                                        string.Format(m_SQLOptimizedChildren, "umbracoNode.parentID = @parentId", "umbracoNode.sortOrder"),
                                        SqlHelper.CreateParameter("@parentId", NodeId)))
            {
                while (dr.Read())
                {
                    Document d = new Document(dr.GetInt("id"), true);
                    bool _hc = false;
                    if (dr.GetInt("children") > 0)
                        _hc = true;
                    int? masterContentType = null;
                    if (!dr.IsNull("masterContentType"))
                        masterContentType = dr.GetInt("masterContentType");
                    d.SetupDocumentForTree(dr.GetGuid("uniqueId")
                        , dr.GetShort("level")
                        , dr.GetInt("parentId")
                        , dr.GetInt("documentUser")
                        , (dr.GetInt("published") == 1)
                        , dr.GetString("path")
                        , dr.GetString("text")
                        , dr.GetDateTime("createDate")
                        , dr.GetDateTime("updateDate")
                        , dr.GetDateTime("versionDate")
                        , dr.GetString("icon")
                        , _hc
                        , dr.GetString("alias")
                        , dr.GetString("thumbnail")
                        , dr.GetString("description")
                        , masterContentType
                        , dr.GetInt("contentTypeId")
                        , dr.GetInt("templateId"));
                    tmp.Add(d);
                }
            }

            Document[] retval = new Document[tmp.Count];

            for (int i = 0; i < tmp.Count; i++)
                retval[i] = (Document)tmp[i];

            return retval;
        }

        private void SetupDocumentForTree(Guid uniqueId, int level, int parentId, int user, bool publish, string path,
                                          string text, DateTime createDate, DateTime updateDate,
                                          DateTime versionDate, string icon, bool hasChildren, string contentTypeAlias, string contentTypeThumb,
                                            string contentTypeDesc, int? masterContentType, int contentTypeId, int templateId)
        {
            SetupNodeForTree(uniqueId, _objectType, level, parentId, user, path, text, createDate, hasChildren);

            _published = publish;
            _updated = updateDate;
            ContentType = new ContentType(contentTypeId, contentTypeAlias, icon, contentTypeThumb, masterContentType);
            ContentTypeIcon = icon;
            Template = templateId;
            VersionDate = versionDate;
        }

        public static void RePublishAll()
        {
            XmlDocument xd = new XmlDocument();
            SqlHelper.ExecuteNonQuery("truncate table cmsContentXml");
            IRecordsReader dr = SqlHelper.ExecuteReader("select nodeId from cmsDocument where published = 1");

            while (dr.Read())
            {
                try
                {
                    new Document(dr.GetInt("nodeId")).XmlGenerate(xd);
                }
                catch (Exception ee)
                {
                    Log.Add(LogTypes.Error, User.GetUser(0), dr.GetInt("nodeId"),
                            string.Format("Error generating xml: {0}", ee));
                }
            }
            dr.Close();
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
                    Log.Add(LogTypes.Error, User.GetUser(0), dr.GetInt("nodeId"),
                            string.Format("Error generating preview xml: {0}", ee));
                }
            }
            dr.Close();
        }

        /// <summary>
        /// Retrieve a list of documents with an expirationdate greater than today
        /// </summary>
        /// <returns>A list of documents with expirationdates than today</returns>
        public static Document[] GetDocumentsForExpiration()
        {
            ArrayList docs = new ArrayList();
            IRecordsReader dr =
                SqlHelper.ExecuteReader("select distinct nodeId from cmsDocument where newest = 1 and not expireDate is null and expireDate <= @today",
                                        SqlHelper.CreateParameter("@today", DateTime.Now));
            while (dr.Read())
                docs.Add(dr.GetInt("nodeId"));
            dr.Close();

            Document[] retval = new Document[docs.Count];
            for (int i = 0; i < docs.Count; i++) retval[i] = new Document((int)docs[i]);
            return retval;
        }

        /// <summary>
        /// Retrieve a list of documents with with releasedate greater than today
        /// </summary>
        /// <returns>Retrieve a list of documents with with releasedate greater than today</returns>
        public static Document[] GetDocumentsForRelease()
        {
            ArrayList docs = new ArrayList();
            IRecordsReader dr = SqlHelper.ExecuteReader("select distinct nodeId, level, sortOrder from cmsDocument inner join umbracoNode on umbracoNode.id = cmsDocument.nodeId where newest = 1 and not releaseDate is null and releaseDate <= @today order by [level], sortOrder",
                                        SqlHelper.CreateParameter("@today", DateTime.Now));
            while (dr.Read())
                docs.Add(dr.GetInt("nodeId"));
            dr.Close();


            Document[] retval = new Document[docs.Count];
            for (int i = 0; i < docs.Count; i++) retval[i] = new Document((int)docs[i]);

            return retval;
        }

        public override List<CMSPreviewNode> GetNodesForPreview(bool childrenOnly)
        {
            List<CMSPreviewNode> nodes = new List<CMSPreviewNode>();

            string pathExp = childrenOnly ? Path + ",%" : Path;

            IRecordsReader dr = SqlHelper.ExecuteReader(String.Format(m_SQLOptimizedForPreview, pathExp));
            while (dr.Read())
                nodes.Add(new CMSPreviewNode(dr.GetInt("id"), dr.GetGuid("versionId"), dr.GetInt("parentId"), dr.GetShort("level"), dr.GetInt("sortOrder"), dr.GetString("xml")));
            dr.Close();

            return nodes;
        }

        /// <summary>
        /// Imports (create) a document from a xmlrepresentation of a document, used by the packager
        /// </summary>
        /// <param name="ParentId">The id to import to</param>
        /// <param name="Creator">Creator of the new document</param>
        /// <param name="Source">Xmlsource</param>
        public static int Import(int ParentId, User Creator, XmlElement Source)
        {
            Document d = MakeNew(
                Source.GetAttribute("nodeName"),
                DocumentType.GetByAlias(Source.GetAttribute("nodeTypeAlias")),
                Creator,
                ParentId);

            d.CreateDateTime = DateTime.Parse(Source.GetAttribute("createDate"));

            // Properties
            foreach (XmlElement n in Source.SelectNodes("data"))
            {
                Property prop = d.getProperty(n.GetAttribute("alias"));
                string propValue = xmlHelper.GetNodeValue(n);

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

            // Subpages
            foreach (XmlElement n in Source.SelectNodes("node"))
                Import(d.Id, Creator, n);

            return d.Id;
        }

        //EVENTS
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
        public static event SaveEventHandler BeforeSave;
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
        public static event SaveEventHandler AfterSave;
        /// <summary>
        /// Raises the <see cref="E:AfterSave"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void FireAfterSave(SaveEventArgs e)
        {
            if (AfterSave != null)
            {
                AfterSave(this, e);
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

        private Dictionary<Property, object> _knownProperties;
        private Func<KeyValuePair<Property, object>, string, bool> propertyTypeByAlias = (pt, alias) => pt.Key.PropertyType.Alias == alias;
        public object this[string alias]
        {
            get
            {
                if (this._optimizedMode)
                {
                    if (this._knownProperties == null) this._knownProperties = new Dictionary<Property, object>();

                    return this._knownProperties.Single(p => propertyTypeByAlias(p, alias)).Value;
                }
                else
                {
                    return this.getProperty(alias).Value;
                }
            }
            set
            {
                if (this._optimizedMode)
                {
                    if (this._knownProperties == null) this._knownProperties = new Dictionary<Property, object>();

                    if (this._knownProperties.SingleOrDefault(p => propertyTypeByAlias(p, alias)).Key == null)
                    {
                        var pt = this.getProperty(alias);

                        this._knownProperties.Add(pt, pt.Value);
                    }
                    else
                    {
                        var pt = this._knownProperties.Single(p => propertyTypeByAlias(p, alias)).Key;
                        this._knownProperties[pt] = value;
                    }
                }
                else
                {
                    this.getProperty(alias).Value = value;
                }
            }
        }
    }

    /// <summary>
    /// A lightweight datastructure used to represent a version of a document
    /// </summary>
    public class DocumentVersionList
    {
        private Guid _version;
        private DateTime _date;
        private string _text;
        private User _user;

        /// <summary>
        /// The unique id of the version
        /// </summary>
        public Guid Version
        {
            get { return _version; }
        }

        /// <summary>
        /// The date of the creation of the version 
        /// </summary>
        public DateTime Date
        {
            get { return _date; }
        }

        /// <summary>
        /// The name of the document in the version
        /// </summary>
        public string Text
        {
            get { return _text; }
        }

        /// <summary>
        /// The user which created the version
        /// </summary>
        public User User
        {
            get { return _user; }
        }

        /// <summary>
        /// Initializes a new instance of the DocumentVersionList class.
        /// </summary>
        /// <param name="Version">Unique version id</param>
        /// <param name="Date">Version createdate</param>
        /// <param name="Text">Version name</param>
        /// <param name="User">Creator</param>
        public DocumentVersionList(Guid Version, DateTime Date, string Text, User User)
        {
            _version = Version;
            _date = Date;
            _text = Text;
            _user = User;
        }
    }
}
