using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using umbraco.DataLayer;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.media;
using System.Threading;

namespace umbraco.cms.businesslogic
{

    public class RecycleBin : CMSNode
    {
        /// <summary>
        /// The types of Recycle Bins.
        /// </summary>
        /// <remarks>
        /// Each enum item represents the integer value of the node Id of the recycle bin in the database.
        /// </remarks>
        public enum RecycleBinType
        {
            Content = -20,
            Media = -21
        }

        private const string m_ChildCountSQL = @"select count(id) from umbracoNode where nodeObjectType = @nodeObjectType and path like '%,{0},%'";
        private const string m_ChildSQL = @"SELECT id, createDate, trashed, parentId, nodeObjectType, nodeUser, level, path, sortOrder, uniqueID, text FROM umbracoNode where ParentID = @parentId And nodeObjectType = @type order by sortOrder";
        private static object m_Locker = new object();

        #region Private variables

        private Guid _nodeObjectType;
        private RecycleBinType m_BinType;

        #endregion

        #region Constructors
        /// <summary>
        /// Constructor to create a new recycle bin 
        /// </summary>
        /// <param name="nodeObjectType"></param>
        /// <param name="nodeId"></param>
        [Obsolete("Use the simple constructor that has the RecycleBinType only parameter")]
        public RecycleBin(Guid nodeObjectType, RecycleBinType type)
            : base((int)type)
        {
            _nodeObjectType = nodeObjectType;
            m_BinType = type;
        }

        /// <summary>
        /// Constructor to create a new recycle bin based on RecycleBinType
        /// Will automatically update internal nodeObjectType based on the RecycleBinType enum
        /// </summary>
        /// <param name="type"></param>
        public RecycleBin(RecycleBinType type)
            : base((int)type)
        {
            switch (type)
            {
                case RecycleBinType.Content:
                    _nodeObjectType = Document._objectType;
                    m_BinType = RecycleBinType.Content;
                    break;
                case RecycleBinType.Media:
                    _nodeObjectType = Media._objectType;
                    m_BinType = RecycleBinType.Media;
                    break;

            }
        }

        /// <summary>
        /// Old constructor to create a content recycle bin
        /// </summary>
        /// <param name="nodeObjectType"></param>
        [Obsolete("Use the simple constructor that has the RecycleBinType only parameter")]
        public RecycleBin(Guid nodeObjectType)
            : this(nodeObjectType, RecycleBinType.Content) { }
        #endregion

        #region Static methods
        /// <summary>
        /// Get the number of items in the Recycle Bin
        /// </summary>
        /// <returns>The number of all items in the Recycle Bin</returns>
        [Obsolete("Create a RecycleBin object to get the count per recycle bin type", true)]
        public static int Count()
        {
            return Count(RecycleBinType.Content);
        }

        public static int Count(RecycleBinType type)
        {
            Guid objectType = Document._objectType;

            switch (type)
            {
                case RecycleBinType.Content:
                    objectType = Document._objectType;
                    break;
                case RecycleBinType.Media:
                    objectType = Media._objectType;
                    break;
            }

            string sql = String.Format(RecycleBin.m_ChildCountSQL,
                        (int) type);

            return SqlHelper.ExecuteScalar<int>(
                    sql,
                    SqlHelper.CreateParameter("@nodeObjectType", objectType));
        }
        #endregion

        #region Public methods

        /// <summary>
        /// If I smell, I'm not empty 
        /// </summary>
        public bool Smells()
        {
            return RecycleBin.Count(m_BinType) > 0;
        }

        /// <summary>
        /// Empties the trash can
        /// </summary>
        /// <param name="itemDeletedCallback">a function to call whenever an item is removed from the bin</param>
        public void CallTheGarbageMan(Action<int> itemDeletedCallback)
        {
            lock (m_Locker)
            {
                //first, move all nodes underneath the recycle bin directly under the recycle bin node (flatten heirarchy)
                //then delete them all.

                SqlHelper.ExecuteNonQuery("UPDATE umbracoNode SET parentID=@parentID, level=1 WHERE path LIKE '%," + ((int)m_BinType).ToString() + ",%'",
                    SqlHelper.CreateParameter("@parentID", (int)m_BinType));

                foreach (var c in Children.ToList())
                {
                    switch (m_BinType)
                    {
                        case RecycleBinType.Content:
                            new Document(c.Id).delete(true);
                            itemDeletedCallback(RecycleBin.Count(m_BinType));
                            break;
                        case RecycleBinType.Media:
                            new Media(c.Id).delete(true);
                            itemDeletedCallback(RecycleBin.Count(m_BinType));
                            break;
                    }
                }
            }
        }

        #endregion

        #region Public properties
        public override umbraco.BusinessLogic.console.IconI[] Children
        {
            get
            {
                System.Collections.ArrayList tmp = new System.Collections.ArrayList();

                using (IRecordsReader dr = SqlHelper.ExecuteReader(m_ChildSQL,
                            SqlHelper.CreateParameter("@parentId", this.Id),
                            SqlHelper.CreateParameter("@type", _nodeObjectType)))
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
        #endregion

    }
}
