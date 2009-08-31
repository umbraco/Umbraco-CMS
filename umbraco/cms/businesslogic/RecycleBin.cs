using System;
using System.Collections.Generic;
using System.Text;

using umbraco.DataLayer;

namespace umbraco.cms.businesslogic
{
    public class RecycleBin : CMSNode
    {
        private Guid _nodeObjectType;

        public RecycleBin(Guid nodeObjectType) : base(-20) {
            _nodeObjectType = nodeObjectType;
        }

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
                
                IRecordsReader dr = SqlHelper.ExecuteReader( "select id from umbracoNode where ParentID = " + this.Id + " And nodeObjectType = @type order by sortOrder",
                                        SqlHelper.CreateParameter("@type",_nodeObjectType));

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
        public static int Count()
        {
            return SqlHelper.ExecuteScalar<int>("select count(id) from umbracoNode where path like '%,-20,%'");
        }

    
    }
}
