﻿using System;
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
            IRecordsReader reader = null;

            try
            {
                //if all is correct
                if (!String.IsNullOrEmpty(group) && !String.IsNullOrEmpty(id))
                {
                    sql = @"SELECT TOP (20) tag FROM cmsTags WHERE tag LIKE @prefix AND cmsTags.id not in 
                        (SELECT tagID FROM cmsTagRelationShip WHERE NodeId = @nodeId) AND cmstags." + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("group") + " = @group;";
                    using (var sqlHelper = Application.SqlHelper)
                    using (var rr = sqlHelper.ExecuteReader(sql,
                        sqlHelper.CreateParameter("@count", count),
                        sqlHelper.CreateParameter("@prefix", prefixText + "%"),
                        sqlHelper.CreateParameter("@nodeId", id),
                        sqlHelper.CreateParameter("@group", group)
                    ))
                    {
                        reader = rr;
                    }
                }
                else
                {
                    //fallback...
                    sql = "SELECT TOP (20) tag FROM cmsTags WHERE tag LIKE @prefix";

                    using (var sqlHelper = Application.SqlHelper)
                    using (var rr = sqlHelper.ExecuteReader(sql,
                        sqlHelper.CreateParameter("@count", count),
                        sqlHelper.CreateParameter("@prefix", prefixText + "%")
                    ))
                    {
                        reader = rr;
                    }
                }

                var tagList = new List<string>();
                while (reader.Read())
                {
                    tagList.Add(reader.GetString("tag"));
                }

                context.Response.Write(returnJson
                    ? new JavaScriptSerializer().Serialize(tagList)
                    : string.Join(Environment.NewLine, tagList));
            }
            catch (Exception ex)
            {
                LogHelper.Error<TagsAutoCompleteHandler>("An error occurred", ex);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }

        }
        
        /// <summary>
        /// Unused, please do not use
        /// </summary>
        [Obsolete("Obsolete, For querying the database use the new UmbracoDatabase object ApplicationContext.Current.DatabaseContext.Database", false)]
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
