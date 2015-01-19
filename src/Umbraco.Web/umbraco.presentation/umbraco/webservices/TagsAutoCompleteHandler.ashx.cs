using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;
using umbraco.DataLayer;
using umbraco.BusinessLogic;
using umbraco.presentation.webservices;

namespace umbraco.presentation.umbraco.webservices
{
    
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class TagsAutoCompleteHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            legacyAjaxCalls.Authorize();

            string format = context.Request.QueryString["format"];
            bool returnJson = format == "json";

            context.Response.ContentType = returnJson ? "application/json" : "text/plain";

            int count = 2;
            string prefixText = context.Request.QueryString["q"];

            if (string.IsNullOrEmpty(prefixText))
                prefixText = context.Request.QueryString["term"];

            string group = context.Request.QueryString["group"];
            string id = context.Request.QueryString["id"];
           
            string sql;

            IRecordsReader rr;

            try
            {
                //if all is correct
                if (!String.IsNullOrEmpty(group) && !String.IsNullOrEmpty(id))
                {
                    sql = @"SELECT TOP (20) tag FROM cmsTags WHERE tag LIKE @prefix AND cmsTags.id not in 
                        (SELECT tagID FROM cmsTagRelationShip WHERE NodeId = @nodeId) AND cmstags." + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("group") + " = @group;";

                    rr = SqlHelper.ExecuteReader(sql,
                        SqlHelper.CreateParameter("@count", count),
                        SqlHelper.CreateParameter("@prefix", prefixText + "%"),
                        SqlHelper.CreateParameter("@nodeId", id),
                        SqlHelper.CreateParameter("@group", group)
                        );



                }
                else
                {
                    //fallback...
                    sql = "SELECT TOP (20) tag FROM cmsTags WHERE tag LIKE @prefix";

                    rr = SqlHelper.ExecuteReader(sql,
                       SqlHelper.CreateParameter("@count", count),
                       SqlHelper.CreateParameter("@prefix", prefixText + "%")
                       );
                }

                var tagList = new List<string>();
                while (rr.Read())
                {
                    tagList.Add(rr.GetString("tag"));
                }

                context.Response.Write(returnJson
                                           ? new JavaScriptSerializer().Serialize(tagList)
                                           : string.Join(Environment.NewLine, tagList));
            }
            catch (Exception ex)
            {
				LogHelper.Error<TagsAutoCompleteHandler>("An error occurred", ex);
            }

        }

        public static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}
