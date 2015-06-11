using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using AutoMapper;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using umbraco;
using Umbraco.Web.WebApi;
using System;
using System.Net.Http.Headers;
using System.Web;
using System.IO;
using Umbraco.Core.Models;
using System.Text.RegularExpressions;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Services;

namespace Umbraco.Web.Editors
{
    
    public class CanvasDesignerController : UmbracoApiController
    {

        [HttpGet]
        public HttpResponseMessage GetGoogleFont()
        {

            // Google Web Font API Key
            var APIKey = "AIzaSyDx7Y58UckkgiETJ_riiTcj7gr_zeCapw4";

            // Google Web Font JSON URL
            var googleWebFontAPIURL = string.Format("https://www.googleapis.com/webfonts/v1/webfonts?key={0}", APIKey);

            var response = "{}";
            using (var client = new System.Net.WebClient())
            {
                response = client.DownloadString(new Uri(googleWebFontAPIURL));
            }

            var resp = Request.CreateResponse();
            resp.Content = new StringContent(response);
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return resp;

        }

        [HttpGet]
        public HttpResponseMessage Load()
        {

            // Get style less url 
            var pageId = int.Parse(HttpContext.Current.Request["pageId"]);

            // Get all parameters
            string paramBlock = CanvasDesignerUtility.GetLessParameters(pageId);

            // Prepare string parameter result
            string[] paramLines = paramBlock.Trim().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            IList<string> parameters = new List<string>();          
            foreach (var line in paramLines)
            {
                if (!line.Contains("@import"))
                    parameters.Add("\"" + line.Replace(":", "\":\"").Replace(";", "\"").Replace("@", "").Replace(";", ""));
            }

            // Response
            var resp = Request.CreateResponse();
            resp.Content = new StringContent("{" + String.Join(",", parameters) + "}");
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return resp;
        }

        [HttpPost]
        public HttpResponseMessage Save()
        {

            // Get parameters
            var parameters = HttpContext.Current.Request["parameters"];
            var pageId = int.Parse(HttpContext.Current.Request["pageId"]);
            var inherited = Boolean.Parse(HttpContext.Current.Request["inherited"]);

            // Save and compile styles
            CanvasDesignerUtility.SaveAndPublishStyle(parameters, pageId, inherited);

            var resp = Request.CreateResponse();
            resp.Content = new StringContent("ok");
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return resp;

        }

        [HttpGet]
        public HttpResponseMessage Delete()
        {

            var pageId = HttpContext.Current.Request["pageId"];

            CanvasDesignerUtility.DeleteStyle(int.Parse(pageId));

            var resp = Request.CreateResponse();
            resp.Content = new StringContent("ok");
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return resp;

        }

        [HttpPost]
        public string Init()
        {

            // Get parameters
            var config = HttpContext.Current.Request["config"];
            var pageId = int.Parse(HttpContext.Current.Request["pageId"]);

            return CanvasDesignerUtility.CreateOrUpdateLessFile(pageId, config);

        }

    }

}