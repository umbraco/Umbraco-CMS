using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence;

using umbraco.cms.businesslogic.web;
using umbraco.DataLayer;
using umbraco.BusinessLogic;
using System.IO;
using System.Text.RegularExpressions;
using System.ComponentModel;
using Umbraco.Core.IO;
using System.Collections;
using umbraco.cms.businesslogic.task;
using Umbraco.Core.Models.Membership;
using File = System.IO.File;

using Task = umbraco.cms.businesslogic.task.Task;

namespace umbraco.cms.businesslogic
{
    /// <summary>
    /// CMSNode class serves as the base class for many of the other components in the cms.businesslogic.xx namespaces.
    /// Providing the basic hierarchical data structure and properties Text (name), Creator, Createdate, updatedate etc.
    /// which are shared by most umbraco objects.
    /// 
    /// The child classes are required to implement an identifier (Guid) which is used as the objecttype identifier, for 
    /// distinguishing the different types of CMSNodes (ex. Documents/Medias/Stylesheets/documenttypes and so forth).
    /// </summary>
    [Obsolete("Obsolete, This class will eventually be phased out", false)]
    public class CMSNode 
    {
        #region Private Members

        private string _text;
        private int _id = 0;
        private Guid _uniqueID;
        private int _parentid;
        private Guid _nodeObjectType;
        private int _level;
        private string _path;
        private bool _hasChildren;
        private int _sortOrder;
        private int _userId;
        private DateTime _createDate;
        private bool _hasChildrenInitialized;
        private string m_image = "default.png";
        private bool? _isTrashed = null;
        protected IUmbracoEntity Entity;

        #endregion

        #region Private static

        
        private static readonly List<string> InternalDefaultIconClasses = new List<string>();
        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();

        private static void InitializeIconClasses()
        {
            
        }
        private const string SqlSingle = "SELECT id, createDate, trashed, parentId, nodeObjectType, nodeUser, level, path, sortOrder, uniqueID, text FROM umbracoNode WHERE id = @id";
        private const string SqlDescendants = @"
            SELECT id, createDate, trashed, parentId, nodeObjectType, nodeUser, level, path, sortOrder, uniqueID, text 
            FROM umbracoNode 
            WHERE path LIKE '%,{0},%'";

        #endregion

        #region Public static

        /// <summary>
        /// Get a count on all CMSNodes given the objecttype
        /// </summary>
        /// <param name="objectType">The objecttype identifier</param>
        /// <returns>
        /// The number of CMSNodes of the given objecttype
        /// </returns>
        public static int CountByObjectType(Guid objectType)
        {
            return SqlHelper.ExecuteScalar<int>("SELECT COUNT(*) from umbracoNode WHERE nodeObjectType = @type", SqlHelper.CreateParameter("@type", objectType));
        }

        /// <summary>
        /// Number of ancestors of the current CMSNode
        /// </summary>
        /// <param name="Id">The CMSNode Id</param>
        /// <returns>
        /// The number of ancestors from the given CMSNode
        /// </returns>
        public static int CountSubs(int Id)
        {
            return SqlHelper.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoNode WHERE ','+path+',' LIKE '%," + Id.ToString() + ",%'");
        }

        /// <summary>
        /// Returns the number of leaf nodes from the newParent id for a given object type
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public static int CountLeafNodes(int parentId, Guid objectType)
        {
            return SqlHelper.ExecuteScalar<int>("Select count(uniqueID) from umbracoNode where nodeObjectType = @type And parentId = @parentId",
                SqlHelper.CreateParameter("@type", objectType),
                SqlHelper.CreateParameter("@parentId", parentId));
        }

        /// <summary>
        /// Gets the default icon classes.
        /// </summary>
        /// <value>The default icon classes.</value>
        public static List<string> DefaultIconClasses
        {
            get
            {
                using (var l = new UpgradeableReadLock(Locker))
                {
                    if (InternalDefaultIconClasses.Count == 0)
                    {
                        l.UpgradeToWriteLock();
                        InitializeIconClasses();
                    }
                    return InternalDefaultIconClasses;
                }

            }
        }

        /// <summary>
        /// Method for checking if a CMSNode exits with the given Guid
        /// </summary>
        /// <param name="uniqueID">Identifier</param>
        /// <returns>True if there is a CMSNode with the given Guid</returns>
        public static bool IsNode(Guid uniqueID)
        {
            return (SqlHelper.ExecuteScalar<int>("select count(id) from umbracoNode where uniqueID = @uniqueID", SqlHelper.CreateParameter("@uniqueId", uniqueID)) > 0);
        }

        /// <summary>
        /// Method for checking if a CMSNode exits with the given id
        /// </summary>
        /// <param name="Id">Identifier</param>
        /// <returns>True if there is a CMSNode with the given id</returns>
        public static bool IsNode(int Id)
        {
            return (SqlHelper.ExecuteScalar<int>("select count(id) from umbracoNode where id = @id", SqlHelper.CreateParameter("@id", Id)) > 0);
        }

        /// <summary>
        /// Retrieve a list of the unique id's of all CMSNodes given the objecttype
        /// </summary>
        /// <param name="objectType">The objecttype identifier</param>
        /// <returns>
        /// A list of all unique identifiers which each are associated to a CMSNode
        /// </returns>
        public static Guid[] getAllUniquesFromObjectType(Guid objectType)
        {
            IRecordsReader dr = SqlHelper.ExecuteReader("Select uniqueID from umbracoNode where nodeObjectType = @type",
                SqlHelper.CreateParameter("@type", objectType));
            System.Collections.ArrayList tmp = new System.Collections.ArrayList();

            while (dr.Read()) tmp.Add(dr.GetGuid("uniqueID"));
            dr.Close();

            Guid[] retval = new Guid[tmp.Count];
            for (int i = 0; i < tmp.Count; i++) retval[i] = (Guid)tmp[i];
            return retval;
        }

        /// <summary>
        /// Retrieve a list of the node id's of all CMSNodes given the objecttype
        /// </summary>
        /// <param name="objectType">The objecttype identifier</param>
        /// <returns>
        /// A list of all node ids which each are associated to a CMSNode
        /// </returns>
        public static int[] getAllUniqueNodeIdsFromObjectType(Guid objectType)
        {
            IRecordsReader dr = SqlHelper.ExecuteReader("Select id from umbracoNode where nodeObjectType = @type",
                SqlHelper.CreateParameter("@type", objectType));
            System.Collections.ArrayList tmp = new System.Collections.ArrayList();

            while (dr.Read()) tmp.Add(dr.GetInt("id"));
            dr.Close();

            return (int[])tmp.ToArray(typeof(int));
        }


        /// <summary>
        /// Retrieves the top level nodes in the hierarchy
        /// </summary>
        /// <param name="ObjectType">The Guid identifier of the type of objects</param>
        /// <returns>
        /// A list of all top level nodes given the objecttype
        /// </returns>
        public static Guid[] TopMostNodeIds(Guid ObjectType)
        {
            IRecordsReader dr = SqlHelper.ExecuteReader("Select uniqueID from umbracoNode where nodeObjectType = @type And parentId = -1 order by sortOrder",
                SqlHelper.CreateParameter("@type", ObjectType));
            System.Collections.ArrayList tmp = new System.Collections.ArrayList();

            while (dr.Read()) tmp.Add(dr.GetGuid("uniqueID"));
            dr.Close();

            Guid[] retval = new Guid[tmp.Count];
            for (int i = 0; i < tmp.Count; i++) retval[i] = (Guid)tmp[i];
            return retval;
        }

        #endregion

        #region Protected static


        /// <summary>
        /// Given the protected modifier the CMSNode.MakeNew method can only be accessed by
        /// derived classes &gt; who by definition knows of its own objectType.
        /// </summary>
        /// <param name="parentId">The newParent CMSNode id</param>
        /// <param name="objectType">The objecttype identifier</param>
        /// <param name="userId">Creator</param>
        /// <param name="level">The level in the tree hieararchy</param>
        /// <param name="text">The name of the CMSNode</param>
        /// <param name="uniqueID">The unique identifier</param>
        /// <returns></returns>
        protected static CMSNode MakeNew(int parentId, Guid objectType, int userId, int level, string text, Guid uniqueID)
        {
            CMSNode parent = null;
            string path = "";
            int sortOrder = 0;

            if (level > 0)
            {
                parent = new CMSNode(parentId);
                sortOrder = GetNewDocumentSortOrder(parentId);
                path = parent.Path;
            }
            else
                path = "-1";

            // Ruben 8/1/2007: I replace this with a parameterized version.
            // But does anyone know what the 'level++' is supposed to be doing there?
            // Nothing obviously, since it's a postfix.

            SqlHelper.ExecuteNonQuery("INSERT INTO umbracoNode(trashed, parentID, nodeObjectType, nodeUser, level, path, sortOrder, uniqueID, text, createDate) VALUES(@trashed, @parentID, @nodeObjectType, @nodeUser, @level, @path, @sortOrder, @uniqueID, @text, @createDate)",
                                      SqlHelper.CreateParameter("@trashed", 0),
                                      SqlHelper.CreateParameter("@parentID", parentId),
                                      SqlHelper.CreateParameter("@nodeObjectType", objectType),
                                      SqlHelper.CreateParameter("@nodeUser", userId),
                                      SqlHelper.CreateParameter("@level", level++),
                                      SqlHelper.CreateParameter("@path", path),
                                      SqlHelper.CreateParameter("@sortOrder", sortOrder),
                                      SqlHelper.CreateParameter("@uniqueID", uniqueID),
                                      SqlHelper.CreateParameter("@text", text),
                                      SqlHelper.CreateParameter("@createDate", DateTime.Now));

            CMSNode retVal = new CMSNode(uniqueID);
            retVal.Path = path + "," + retVal.Id.ToString();

            // NH 4.7.1 duplicate permissions because of refactor
            if (parent != null)
            {
                IEnumerable<Permission> permissions = Permission.GetNodePermissions(parent);
                foreach (Permission p in permissions)
                {
                    Permission.MakeNew(ApplicationContext.Current.Services.UserService.GetUserById(p.UserId), retVal, p.PermissionId);
                }

            }

            
            return retVal;
        }

        private static int GetNewDocumentSortOrder(int parentId)
        {
            var sortOrder = 0;
            using (IRecordsReader dr = SqlHelper.ExecuteReader(
                        "SELECT MAX(sortOrder) AS sortOrder FROM umbracoNode WHERE parentID = @parentID AND nodeObjectType = @GuidForNodesOfTypeDocument",
                        SqlHelper.CreateParameter("@parentID", parentId),
                        SqlHelper.CreateParameter("@GuidForNodesOfTypeDocument", Document._objectType)
                  ))
            {
                while (dr.Read())
                    sortOrder = dr.GetInt("sortOrder") + 1;
            }

            return sortOrder;
        }

        /// <summary>
        /// Retrieve a list of the id's of all CMSNodes given the objecttype and the first letter of the name.
        /// </summary>
        /// <param name="objectType">The objecttype identifier</param>
        /// <param name="letter">Firstletter</param>
        /// <returns>
        /// A list of all CMSNodes which has the objecttype and a name that starts with the given letter
        /// </returns>
        protected static int[] getUniquesFromObjectTypeAndFirstLetter(Guid objectType, char letter)
        {
            using (IRecordsReader dr = SqlHelper.ExecuteReader("Select id from umbracoNode where nodeObjectType = @objectType AND text like @letter", SqlHelper.CreateParameter("@objectType", objectType), SqlHelper.CreateParameter("@letter", letter.ToString() + "%")))
            {
                List<int> tmp = new List<int>();
                while (dr.Read()) tmp.Add(dr.GetInt("id"));
                return tmp.ToArray();
            }
        }


        /// <summary>
        /// Gets the SQL helper.
        /// </summary>
        /// <value>The SQL helper.</value>
        [Obsolete("Obsolete, For querying the database use the new UmbracoDatabase object ApplicationContext.Current.DatabaseContext.Database", false)]
        protected static ISqlHelper SqlHelper
        {
            get { return LegacySqlHelper.SqlHelper; }
        }
        #endregion

        #region Constructors

        /// <summary>
        /// Empty constructor that is not suported
        /// ...why is it here?
        /// </summary>
        public CMSNode()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CMSNode"/> class.
        /// </summary>
        /// <param name="Id">The id.</param>
        public CMSNode(int Id)
        {
            _id = Id;
            setupNode();
        }

        /// <summary>
        /// This is purely for a hackity hack hack hack in order to make the new Document(id, version) constructor work because
        /// the Version property needs to be set on the object before setupNode is called, otherwise it never works! this allows
        /// inheritors to set default data before setupNode() is called.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ctorArgs"></param>
        internal CMSNode(int id, object[] ctorArgs)
        {
            _id = id;
            PreSetupNode(ctorArgs);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CMSNode"/> class.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="noSetup">if set to <c>true</c> [no setup].</param>
        public CMSNode(int id, bool noSetup)
        {
            _id = id;

            if (!noSetup)
                setupNode();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CMSNode"/> class.
        /// </summary>
        /// <param name="uniqueID">The unique ID.</param>
        public CMSNode(Guid uniqueID)
        {
            _id = SqlHelper.ExecuteScalar<int>("SELECT id FROM umbracoNode WHERE uniqueID = @uniqueId", SqlHelper.CreateParameter("@uniqueId", uniqueID));
            setupNode();
        }

        public CMSNode(Guid uniqueID, bool noSetup)
        {
            _id = SqlHelper.ExecuteScalar<int>("SELECT id FROM umbracoNode WHERE uniqueID = @uniqueId", SqlHelper.CreateParameter("@uniqueId", uniqueID));

            if (!noSetup)
                setupNode();
        }

        protected internal CMSNode(IRecordsReader reader)
        {
            _id = reader.GetInt("id");
            PopulateCMSNodeFromReader(reader);
        }

        protected internal CMSNode(IUmbracoEntity entity)
        {
            _id = entity.Id;
            Entity = entity;
        }

        protected internal CMSNode(IEntity entity)
        {
            _id = entity.Id;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Ensures uniqueness by id
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var l = obj as CMSNode;
            if (l != null)
            {
                return this._id.Equals(l._id);
            }
            return false;
        }

        /// <summary>
        /// Ensures uniqueness by id
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        /// <summary>
        /// An xml representation of the CMSNOde
        /// </summary>
        /// <param name="xd">Xmldocument context</param>
        /// <param name="Deep">If true the xml will append the CMSNodes child xml</param>
        /// <returns>The CMSNode Xmlrepresentation</returns>
        public virtual XmlNode ToXml(XmlDocument xd, bool Deep)
        {
            throw new NotSupportedException("DO NOT USE THIS METHOD, THIS NEEDS TO BE REMOVED FROM THE CODEBASE, REFACTOR ANYTHING THAT IS USING THIS IF YOU GET THIS EXCEPTION");
        }

        public virtual XmlNode ToPreviewXml(XmlDocument xd)
        {
            // If xml already exists
            if (!PreviewExists(UniqueId))
            {
                SavePreviewXml(ToXml(xd, false), UniqueId);
            }
            return GetPreviewXml(xd, UniqueId);
        }

        public virtual List<CMSPreviewNode> GetNodesForPreview(bool childrenOnly)
        {
            List<CMSPreviewNode> nodes = new List<CMSPreviewNode>();
            string sql = @"
select umbracoNode.id, umbracoNode.parentId, umbracoNode.level, umbracoNode.sortOrder, cmsPreviewXml.xml
from umbracoNode 
inner join cmsPreviewXml on cmsPreviewXml.nodeId = umbracoNode.id 
where trashed = 0 and path like '{0}' 
order by level,sortOrder";

            string pathExp = childrenOnly ? Path + ",%" : Path;

            IRecordsReader dr = SqlHelper.ExecuteReader(String.Format(sql, pathExp));
            while (dr.Read())
                nodes.Add(new CMSPreviewNode(dr.GetInt("id"), dr.GetGuid("uniqueID"), dr.GetInt("parentId"), dr.GetShort("level"), dr.GetInt("sortOrder"), dr.GetString("xml"), false));
            dr.Close();

            return nodes;
        }

        /// <summary>
        /// Used to persist object changes to the database. In Version3.0 it's just a stub for future compatibility
        /// </summary>
        public virtual void Save()
        {
           
        }

        public override string ToString()
        {
            if (Id != int.MinValue || !string.IsNullOrEmpty(Text))
            {
                return string.Format("{{ Id: {0}, Text: {1}, ParentId: {2} }}",
                    Id,
                    Text,
                    _parentid
                );
            }

            return base.ToString();
        }


        [EditorBrowsable(EditorBrowsableState.Never)]

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        public virtual void delete()
        {
           

            //removes tasks
            foreach (Task t in Tasks)
            {
                t.Delete();
            }
            
            //remove permissions
            Permission.DeletePermissions(this);

            ////removes tag associations (i know the key is set to cascade but do it anyways)
            //Tag.RemoveTagsFromNode(this.Id);

            SqlHelper.ExecuteNonQuery("DELETE FROM umbracoNode WHERE uniqueID= @uniqueId", SqlHelper.CreateParameter("@uniqueId", _uniqueID));
        }

        /// <summary>
        /// Does the current CMSNode have any child nodes.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has children; otherwise, <c>false</c>.
        /// </value>
        public virtual bool HasChildren
        {
            get
            {
                if (!_hasChildrenInitialized)
                {
                    int tmpChildrenCount = SqlHelper.ExecuteScalar<int>("select count(id) from umbracoNode where ParentId = @id",
                        SqlHelper.CreateParameter("@id", Id));
                    HasChildren = (tmpChildrenCount > 0);
                }
                return _hasChildren;
            }
            set
            {
                _hasChildrenInitialized = true;
                _hasChildren = value;
            }
        }

        /// <summary>
        /// Returns all descendant nodes from this node.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This doesn't return a strongly typed IEnumerable object so that we can override in in super clases
        /// and since this class isn't a generic (thought it should be) this is not strongly typed.
        /// </remarks>
        public virtual IEnumerable GetDescendants()
        {
            var descendants = new List<CMSNode>();
            using (IRecordsReader dr = SqlHelper.ExecuteReader(string.Format(SqlDescendants, Id)))
            {
                while (dr.Read())
                {
                    var node = new CMSNode(dr.GetInt("id"), true);
                    node.PopulateCMSNodeFromReader(dr);
                    descendants.Add(node);
                }
            }
            return descendants;
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Determines if the node is in the recycle bin.
        /// This is only relavent for node types that support a recyle bin (such as Document/Media)
        /// </summary>
        public virtual bool IsTrashed
        {
            get
            {
                if (!_isTrashed.HasValue)
                {
                    _isTrashed = Convert.ToBoolean(SqlHelper.ExecuteScalar<object>("SELECT trashed FROM umbracoNode where id=@id",
                        SqlHelper.CreateParameter("@id", this.Id)));
                }
                return _isTrashed.Value;
            }
            set
            {
                _isTrashed = value;
                SqlHelper.ExecuteNonQuery("update umbracoNode set trashed = @trashed where id = @id",
                    SqlHelper.CreateParameter("@trashed", value),
                    SqlHelper.CreateParameter("@id", this.Id));
            }
        }

        /// <summary>
        /// Gets or sets the sort order.
        /// </summary>
        /// <value>The sort order.</value>
        public virtual int sortOrder
        {
            get { return _sortOrder; }
            set
            {
                _sortOrder = value;
                SqlHelper.ExecuteNonQuery("update umbracoNode set sortOrder = '" + value + "' where id = " + this.Id.ToString());

                if (Entity != null)
                    Entity.SortOrder = value;
            }
        }

        /// <summary>
        /// Gets or sets the create date time.
        /// </summary>
        /// <value>The create date time.</value>
        public virtual DateTime CreateDateTime
        {
            get { return _createDate; }
            set
            {
                _createDate = value;
                SqlHelper.ExecuteNonQuery("update umbracoNode set createDate = @createDate where id = " + this.Id.ToString(), SqlHelper.CreateParameter("@createDate", _createDate));
            }
        }

        /// <summary>
        /// Gets the creator
        /// </summary>
        /// <value>The user.</value>
        public IUser User
        {
            get
            {
                return ApplicationContext.Current.Services.UserService.GetUserById(_userId);
            }
        }

        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <value>The id.</value>
        public int Id
        {
            get { return _id; }
        }

        /// <summary>
        /// Get the newParent id of the node
        /// </summary>
        public virtual int ParentId
        {
            get { return _parentid; }
            internal set { _parentid = value; }
        }

        private IUmbracoEntity _parent;
        /// <summary>
        /// Given the hierarchical tree structure a CMSNode has only one newParent but can have many children
        /// </summary>
        /// <value>The newParent.</value>
        public CMSNode Parent
        {
            get
            {
                if (Level == 1) throw new ArgumentException("No newParent node");
                if (_parent == null)
                {
                    _parent = ApplicationContext.Current.Services.EntityService.Get(_parentid);
                }
                return new CMSNode(_parent);
            }
            set
            {
                _parentid = value.Id;
                SqlHelper.ExecuteNonQuery("update umbracoNode set parentId = " + value.Id + " where id = " + this.Id.ToString());

                if (Entity != null)
                    Entity.ParentId = value.Id;

                _parent = value.Entity;
            }
        }

        /// <summary>
        /// An comma separated string consisting of integer node id's
        /// that indicates the path from the topmost node to the given node
        /// </summary>
        /// <value>The path.</value>
        public virtual string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                SqlHelper.ExecuteNonQuery("update umbracoNode set path = '" + _path + "' where id = " + this.Id.ToString());

                if (Entity != null)
                    Entity.Path = value;
            }
        }

        /// <summary>
        /// Returns an integer value that indicates in which level of the
        /// tree structure the given node is
        /// </summary>
        /// <value>The level.</value>
        public virtual int Level
        {
            get { return _level; }
            set
            {
                _level = value;
                SqlHelper.ExecuteNonQuery("update umbracoNode set level = " + _level.ToString() + " where id = " + this.Id.ToString());

                if (Entity != null)
                    Entity.Level = value;
            }
        }

        /// <summary>
        /// All CMSNodes has an objecttype ie. Webpage, StyleSheet etc., used to distinguish between the different
        /// object types for for fast loading children to the tree.
        /// </summary>
        /// <value>The type of the node object.</value>
        public Guid nodeObjectType
        {
            get { return _nodeObjectType; }
        }

        /// <summary>
        /// Returns all tasks associated with this node
        /// </summary>
        public Tasks Tasks
        {
            get { return Task.GetTasks(this.Id); }
        }

        public virtual int ChildCount
        {
            get
            {
                return SqlHelper.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoNode where ParentID = @parentId",
                                                    SqlHelper.CreateParameter("@parentId", this.Id));
            }
        }

        /// <summary>
        /// The basic recursive tree pattern
        /// </summary>
        /// <value>The children.</value>
        public virtual CMSNode[] Children
        {
            get
            {
                System.Collections.ArrayList tmp = new System.Collections.ArrayList();
                using (IRecordsReader dr = SqlHelper.ExecuteReader("SELECT id, createDate, trashed, parentId, nodeObjectType, nodeUser, level, path, sortOrder, uniqueID, text FROM umbracoNode WHERE ParentID = @ParentID AND nodeObjectType = @type order by sortOrder",
                    SqlHelper.CreateParameter("@type", this.nodeObjectType),
                    SqlHelper.CreateParameter("ParentID", this.Id)))
                {
                    while (dr.Read())
                    {
                        tmp.Add(new CMSNode(dr));
                    }
                }

                CMSNode[] retval = new CMSNode[tmp.Count];

                for (int i = 0; i < tmp.Count; i++)
                {
                    retval[i] = (CMSNode)tmp[i];
                }
                return retval;
            }
        }

        /// <summary>
        /// Retrieve all CMSNodes in the umbraco installation
        /// Use with care.
        /// </summary>
        /// <value>The children of all object types.</value>
        public CMSNode[] ChildrenOfAllObjectTypes
        {
            get
            {
                System.Collections.ArrayList tmp = new System.Collections.ArrayList();
                IRecordsReader dr = SqlHelper.ExecuteReader("select id from umbracoNode where ParentID = " + this.Id + " order by sortOrder");

                while (dr.Read())
                    tmp.Add(dr.GetInt("Id"));

                dr.Close();

                CMSNode[] retval = new CMSNode[tmp.Count];

                for (int i = 0; i < tmp.Count; i++)
                    retval[i] = new CMSNode((int)tmp[i]);

                return retval;
            }
        }

        #region IconI members

        // Unique identifier of the given node
        /// <summary>
        /// Unique identifier of the CMSNode, used when locating data.
        /// </summary>
        public Guid UniqueId
        {
            get { return _uniqueID; }
        }

        /// <summary>
        /// Human readable name/label
        /// </summary>
        public virtual string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                SqlHelper.ExecuteNonQuery("UPDATE umbracoNode SET text = @text WHERE id = @id",
                                          SqlHelper.CreateParameter("@text", value.Trim()),
                                          SqlHelper.CreateParameter("@id", this.Id));

                if (Entity != null)
                    Entity.Name = value;
            }
        }

        /// <summary>
        /// Not implemented, always returns "about:blank"
        /// </summary>
        public virtual string DefaultEditorURL
        {
            get { return "about:blank"; }
        }

        /// <summary>
        /// The icon in the tree
        /// </summary>
        public virtual string Image
        {
            get { return m_image; }
            set { m_image = value; }

        }

        /// <summary>
        /// The "open/active" icon in the tree
        /// </summary>
        public virtual string OpenImage
        {
            get { return ""; }
        }

        #endregion

        #endregion

        #region Protected methods

        /// <summary>
        /// This allows inheritors to set the underlying text property without persisting the change to the database.
        /// </summary>
        /// <param name="txt"></param>
        protected void SetText(string txt)
        {
            _text = txt;

            if (Entity != null)
                Entity.Name = txt;
        }

        /// <summary>
        /// This is purely for a hackity hack hack hack in order to make the new Document(id, version) constructor work because
        /// the Version property needs to be set on the object before setupNode is called, otherwise it never works!
        /// </summary>
        /// <param name="ctorArgs"></param>
        internal virtual void PreSetupNode(params object[] ctorArgs)
        {
            //if people want to override then awesome but then we call setupNode so they need to ensure
            // to call base.PreSetupNode
            setupNode();
        }

        /// <summary>
        /// Sets up the internal data of the CMSNode, used by the various constructors
        /// </summary>
        protected virtual void setupNode()
        {
            using (IRecordsReader dr = SqlHelper.ExecuteReader(SqlSingle,
                    SqlHelper.CreateParameter("@id", this.Id)))
            {
                if (dr.Read())
                {
                    PopulateCMSNodeFromReader(dr);
                }
                else
                {
                    throw new ArgumentException(string.Format("No node exists with id '{0}'", Id));
                }
            }
        }

        /// <summary>
        /// Sets up the node for the content tree, this makes no database calls, just sets the underlying properties
        /// </summary>
        /// <param name="uniqueID">The unique ID.</param>
        /// <param name="nodeObjectType">Type of the node object.</param>
        /// <param name="Level">The level.</param>
        /// <param name="ParentId">The newParent id.</param>
        /// <param name="UserId">The user id.</param>
        /// <param name="Path">The path.</param>
        /// <param name="Text">The text.</param>
        /// <param name="CreateDate">The create date.</param>
        /// <param name="hasChildren">if set to <c>true</c> [has children].</param>
        protected void SetupNodeForTree(Guid uniqueID, Guid nodeObjectType, int leve, int parentId, int userId, string path, string text,
            DateTime createDate, bool hasChildren)
        {
            _uniqueID = uniqueID;
            _nodeObjectType = nodeObjectType;
            _level = leve;
            _parentid = parentId;
            _userId = userId;
            _path = path;
            _text = text;
            _createDate = createDate;
            HasChildren = hasChildren;
        }

        /// <summary>
        /// Updates the temp path for the content tree.
        /// </summary>
        /// <param name="Path">The path.</param>
        protected void UpdateTempPathForTree(string Path)
        {
            this._path = Path;
        }

        protected virtual XmlNode GetPreviewXml(XmlDocument xd, Guid version)
        {

            XmlDocument xmlDoc = new XmlDocument();
            using (XmlReader xmlRdr = SqlHelper.ExecuteXmlReader(
                                                       "select xml from cmsPreviewXml where nodeID = @nodeId and versionId = @versionId",
                                      SqlHelper.CreateParameter("@nodeId", Id),
                                      SqlHelper.CreateParameter("@versionId", version)))
            {
                xmlDoc.Load(xmlRdr);
            }

            return xd.ImportNode(xmlDoc.FirstChild, true);
        }

        protected internal virtual bool PreviewExists(Guid versionId)
        {
            return (SqlHelper.ExecuteScalar<int>("SELECT COUNT(nodeId) FROM cmsPreviewXml WHERE nodeId=@nodeId and versionId = @versionId",
                        SqlHelper.CreateParameter("@nodeId", Id), SqlHelper.CreateParameter("@versionId", versionId)) != 0);

        }

        /// <summary>
        /// This needs to be synchronized since we are doing multiple sql operations in one method
        /// </summary>
        /// <param name="x"></param>
        /// <param name="versionId"></param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected void SavePreviewXml(XmlNode x, Guid versionId)
        {
            string sql = PreviewExists(versionId) ? "UPDATE cmsPreviewXml SET xml = @xml, timestamp = @timestamp WHERE nodeId=@nodeId AND versionId = @versionId"
                                : "INSERT INTO cmsPreviewXml(nodeId, versionId, timestamp, xml) VALUES (@nodeId, @versionId, @timestamp, @xml)";
            SqlHelper.ExecuteNonQuery(sql,
                                      SqlHelper.CreateParameter("@nodeId", Id),
                                      SqlHelper.CreateParameter("@versionId", versionId),
                                      SqlHelper.CreateParameter("@timestamp", DateTime.Now),
                                      SqlHelper.CreateParameter("@xml", x.OuterXml));
        }

        protected void PopulateCMSNodeFromReader(IRecordsReader dr)
        {
            // testing purposes only > original umbraco data hasn't any unique values ;)
            // And we need to have a newParent in order to create a new node ..
            // Should automatically add an unique value if no exists (or throw a decent exception)
            if (dr.IsNull("uniqueID")) _uniqueID = Guid.NewGuid();
            else _uniqueID = dr.GetGuid("uniqueID");

            _nodeObjectType = dr.GetGuid("nodeObjectType");
            _level = dr.GetShort("level");
            _path = dr.GetString("path");
            _parentid = dr.GetInt("parentId");
            _text = dr.GetString("text");
            _sortOrder = dr.GetInt("sortOrder");
            _userId = dr.GetInt("nodeUser");
            _createDate = dr.GetDateTime("createDate");
            _isTrashed = dr.GetBoolean("trashed");
        }

        internal protected void PopulateCMSNodeFromUmbracoEntity(IUmbracoEntity content, Guid objectType)
        {
            _uniqueID = content.Key;
            _nodeObjectType = objectType;
            _level = content.Level;
            _path = content.Path;
            _parentid = content.ParentId;
            _text = content.Name;
            _sortOrder = content.SortOrder;
            _userId = content.CreatorId;
            _createDate = content.CreateDate;
            _isTrashed = content.Trashed;
            Entity = content;
        }

        internal protected void PopulateCMSNodeFromUmbracoEntity(IAggregateRoot content, Guid objectType)
        {
            _uniqueID = content.Key;
            _nodeObjectType = objectType;            
            _createDate = content.CreateDate;
        }

        #endregion


    }
}