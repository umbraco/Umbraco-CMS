using System;
using System.Data;
using System.Web;
using System.Collections;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.ComponentModel;

using umbraco.DataLayer;

namespace umbracoTags.webservice {
    /// <summary>
    /// Summary description for tagService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]
    public class tagService : System.Web.Services.WebService {

        private static ISqlHelper _sqlHelper;
        private static string _ConnString = umbraco.GlobalSettings.DbDSN;

        public static ISqlHelper SqlHelper
        {
            get
            {
                if (_sqlHelper == null)
                {
                    try
                    {
                        _sqlHelper = DataLayerHelper.CreateSqlHelper(_ConnString);
                    }
                    catch { }
                }
                return _sqlHelper;
            }
        }


        /// <summary>
        /// Gets the tag list.
        /// </summary>
        /// <param name="prefixText">The prefix text.</param>
        /// <param name="count">The count.</param>
        /// <returns>string array.</returns>
        [WebMethod]
        public string[] getTagList(string prefixText, int count) {

                IRecordsReader rr = SqlHelper.ExecuteReader("SELECT TOP 20 tag FROM cmsTags WHERE tag LIKE @prefix;",
                    SqlHelper.CreateParameter("@count", count), 
                    SqlHelper.CreateParameter("@prefix", prefixText + "%" ) 
                    );
                string result = "";

                while (rr.Read()) {
                    result += rr.GetString("tag") + ",";
                }

                return result.Trim(',').Split(',');
        }

        /// <summary>
        /// Gets the tag list.
        /// </summary>
        /// <param name="prefixText">The prefix text.</param>
        /// <param name="count">The count.</param>
        /// <param name="contextKey">The context key.</param>
        /// <returns>string array</returns>
        [WebMethod]
        public string[] getTagList(string prefixText, int count, string contextKey)
        {
            umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, prefixText + " " + count + " " + contextKey);

            string[] groupAndId = contextKey.Trim().Split('|');
            //fallback...
            string sql;

            IRecordsReader rr;

            try {
                //if all is correct
                if (groupAndId.Length == 2) {
                    sql = @"SELECT TOP 20 tag FROM cmsTags WHERE tag LIKE @prefix AND cmsTags.id not in 
                        (SELECT tagID FROM cmsTagRelationShip WHERE NodeId = @nodeId) AND cmstags.[group] = @group;";

                    rr = SqlHelper.ExecuteReader(sql,
                        SqlHelper.CreateParameter("@count", count),
                        SqlHelper.CreateParameter("@prefix", prefixText + "%"),
                        SqlHelper.CreateParameter("@nodeId", groupAndId[0]),
                        SqlHelper.CreateParameter("@group", groupAndId[1])
                        );


                    umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "correct param " + groupAndId[1] + " " + groupAndId[0]);


                } else {
                    //fallback...
                    sql = "SELECT TOP 20 tag FROM cmsTags WHERE tag LIKE @prefix";

                    rr = SqlHelper.ExecuteReader(sql,
                       SqlHelper.CreateParameter("@count", count),
                       SqlHelper.CreateParameter("@prefix", prefixText + "%")
                       );
                }

                string result = "";

                while (rr.Read()) {
                    result += rr.GetString("tag") + ",";
                }


                umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "result");
                return result.Trim(',').Split(',');

            } catch (Exception ex) {
                umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, ex.ToString());
            }

            return ("jaxx,pik,hat").Split(',');
        }

    }
}
