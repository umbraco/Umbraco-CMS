using System;
using System.Linq;
using umbraco.BusinessLogic;
using Umbraco.Core;
using umbraco.DataLayer;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.media;

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
            Content = Constants.System.RecycleBinContent,
            Media = Constants.System.RecycleBinMedia
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
        /// <param name="type"></param>
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

            using (var sqlHelper = Application.SqlHelper)
                return sqlHelper.ExecuteScalar<int>(sql, sqlHelper.CreateParameter("@nodeObjectType", objectType));
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
                var itemsInTheBin = Count(m_BinType);
                itemDeletedCallback(itemsInTheBin);

                if (m_BinType == RecycleBinType.Media)
                {
                    ApplicationContext.Current.Services.MediaService.EmptyRecycleBin();
                    var trashedMedia = ApplicationContext.Current.Services.MediaService.GetMediaInRecycleBin();
                    itemsInTheBin = trashedMedia.Count();
                }
                else
                {
                    ApplicationContext.Current.Services.ContentService.EmptyRecycleBin();
                    var trashedContent = ApplicationContext.Current.Services.ContentService.GetContentInRecycleBin();
                    itemsInTheBin = trashedContent.Count();
                }

                itemDeletedCallback(itemsInTheBin);
            }
        }

        #endregion

        #region Public properties
        public override umbraco.BusinessLogic.console.IconI[] Children
        {
            get
            {
                System.Collections.ArrayList tmp = new System.Collections.ArrayList();

                using (var sqlHelper = Application.SqlHelper)
                using (IRecordsReader dr = sqlHelper.ExecuteReader(m_ChildSQL,
                            sqlHelper.CreateParameter("@parentId", this.Id),
                            sqlHelper.CreateParameter("@type", _nodeObjectType)))
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
