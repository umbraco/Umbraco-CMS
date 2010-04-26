using System;
using System.Collections.Generic;
using System.Text;

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
            Content = -20,
            Media = -21
        }

        private Guid _nodeObjectType;

        /// <summary>
        /// Constructor to create a new recycle bin 
        /// </summary>
        /// <param name="nodeObjectType"></param>
        /// <param name="nodeId"></param>
        public RecycleBin(Guid nodeObjectType, RecycleBinType type)
            : base((int)type)
        {
            _nodeObjectType = nodeObjectType;
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
                    break;
                case RecycleBinType.Media:
                    _nodeObjectType = Media._objectType;
                    break;

            }
        }

        /// <summary>
        /// Old constructor to create a content recycle bin
        /// </summary>
        /// <param name="nodeObjectType"></param>
        [Obsolete("Use the other constructors instead")]
        public RecycleBin(Guid nodeObjectType)
            : this(nodeObjectType, RecycleBinType.Content) { }


        /// <summary>
        /// If I smell, I'm not empty 
        /// </summary>
        public bool Smells()
        {
            return (SqlHelper.ExecuteScalar<int>(
                    "select count(id) from umbracoNode where nodeObjectType = @nodeObjectType and parentId = @parentId",
                    SqlHelper.CreateParameter("@parentId", this.Id),
                    SqlHelper.CreateParameter("@nodeObjectType", _nodeObjectType)) > 0);

        }

        public override umbraco.BusinessLogic.console.IconI[] Children
        {
            get
            {
                System.Collections.ArrayList tmp = new System.Collections.ArrayList();

                IRecordsReader dr = SqlHelper.ExecuteReader("select id from umbracoNode where ParentID = " + this.Id + " And nodeObjectType = @type order by sortOrder",
                                        SqlHelper.CreateParameter("@type", _nodeObjectType));

                while (dr.Read())
                    tmp.Add(dr.GetInt("Id"));

                dr.Close();

                CMSNode[] retval = new CMSNode[tmp.Count];

                for (int i = 0; i < tmp.Count; i++)
                    retval[i] = new CMSNode((int)tmp[i]);

                return retval;
            }
        }

        /// <summary>
        /// Get the number of items in the Recycle Bin
        /// </summary>
        /// <returns>The number of all items in the Recycle Bin</returns>
        [Obsolete("Create a RecycleBin object to get the count per recycle bin type", true)]
        public static int Count()
        {
            return SqlHelper.ExecuteScalar<int>("select count(id) from umbracoNode where path like '%,-20,%'");
        }

        public static int Count(RecycleBinType type)
        {
            string sql = String.Format("select count(id) from umbracoNode where path like '%,{0},%'", (int)type);
            return SqlHelper.ExecuteScalar<int>(sql);
        }


    }
}
