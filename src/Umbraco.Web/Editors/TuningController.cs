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

namespace Umbraco.Web.Editors
{
    
    public class TuningController : UmbracoApiController
    {

        static string basePath = HttpContext.Current.Server.MapPath(@"\Umbraco\assets\less\");
        static string frontBasePath = HttpContext.Current.Server.MapPath(@"\Css\tuning\");
        static string resultCssPath = @"\Css\tuning\{0}.css";
        static string resultLessPath = @"\Css\tuning\{0}.less";
        static string resultGridLessPath = @"\Css\tuning\grid_{0}.less";
        static string resultGridCssPath = @"\Css\tuning\grid_{0}.css";
        static string tuningStylePath = basePath + @"tuning.defaultStyle.less";

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

            var resp = new HttpResponseMessage()
            {
                Content = new StringContent(response)
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return resp;

        }

        [HttpGet]
        public HttpResponseMessage Load()
        {

            // Get style less url 
            var tuningStyleUrl = HttpContext.Current.Request["tuningStyleUrl"];
            var tuningGridStyleUrl = HttpContext.Current.Request["tuningGridStyleUrl"];

            if (string.IsNullOrEmpty(tuningStyleUrl))
                tuningStyleUrl = tuningStylePath;
            else
                tuningStyleUrl = HttpContext.Current.Server.MapPath(tuningStyleUrl);

            // Get all parameters
            string paramBlock = string.Empty;
            using (System.IO.StreamReader sr = new System.IO.StreamReader(tuningStyleUrl))
            {
                paramBlock = GetStyleBloque("lessParam", sr.ReadToEnd());
            }
            if (!string.IsNullOrEmpty(tuningGridStyleUrl))
            {
                using (System.IO.StreamReader sr = new System.IO.StreamReader(HttpContext.Current.Server.MapPath(tuningGridStyleUrl)))
                {
                    paramBlock += GetStyleBloque("lessParam", sr.ReadToEnd());
                }
            }

            // Prepare string parameter result
            string[] paramLines = paramBlock.Trim().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            IList<string> parameters = new List<string>();          
            foreach (var line in paramLines)
            {
                if (!line.Contains("@import"))
                    parameters.Add("\"" + line.Replace(":", "\":\"").Replace(";", "\"").Replace("@", "").Replace(";", ""));
            }

            // Response
            var resp = new HttpResponseMessage()
            {
                Content = new StringContent("{" + String.Join(",", parameters) + "}")
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return resp;
        }

        [HttpPost]
        public HttpResponseMessage Save()
        {

            // Get parameters
            var parameters = HttpContext.Current.Request["parameters"];
            var parametersGrid = HttpContext.Current.Request["parametersGrid"];
            var pageId = HttpContext.Current.Request["pageId"];

            /*********************************************/
            /* Grid parameters */
            /*********************************************/

            // Path to the new grid less
            string newResultGridLessPath = HttpContext.Current.Server.MapPath(string.Format(resultGridLessPath, pageId));
            string newResultGridCssPath = HttpContext.Current.Server.MapPath(string.Format(resultGridCssPath, pageId));

            // Load current grid Less
            string gridLessContent = string.Empty;
            using (System.IO.StreamReader sr = new System.IO.StreamReader(newResultGridLessPath))
            {
                gridLessContent = sr.ReadToEnd();
            }

            // Prepare grid parameters
            string newGridParamBlock = string.Empty;
            foreach (string parameter in parametersGrid.Trim().Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
            {
                newGridParamBlock += (parameter + ";").Replace(":;", ":'';") + Environment.NewLine;
            }
            gridLessContent = gridLessContent.Replace(GetStyleBloque("lessParam", gridLessContent), Environment.NewLine + newGridParamBlock);

            // Save Grid less file
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(newResultGridLessPath))
            {
                file.Write(gridLessContent);
            }

            // Compile the Grid Less file
            string compiledGridStyle = GetCssFromLessString(gridLessContent, false, true, true);

            // Save compiled file
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(newResultGridCssPath))
            {
                file.Write(compiledGridStyle);
            }

            

            /*********************************************/
            /* Main parameters */
            /*********************************************/

            // Path to the new less and css files
            string newResultLessPath = HttpContext.Current.Server.MapPath(string.Format(resultLessPath, pageId));
            string newResultCssPath = HttpContext.Current.Server.MapPath(string.Format(resultCssPath, pageId));

            // Load default Less content
            string lessContent = string.Empty;
            using (System.IO.StreamReader sr = new System.IO.StreamReader(tuningStylePath))
            {
                lessContent = sr.ReadToEnd();
            }

            // Create front directory
            if (!Directory.Exists(frontBasePath))
                Directory.CreateDirectory(frontBasePath);

            // Prepare parameters and gf block
            string newParamBlock = string.Empty;
            string newGfBlock = string.Empty;
            foreach (string parameter in parameters.Trim().Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (parameter.IndexOf("@import") < 0)
                {
                    newParamBlock += (parameter + ";").Replace(":;", ":'';") + Environment.NewLine;
                }
                else 
                {
                    newGfBlock += parameter + ";" + Environment.NewLine;
                }
            }
            lessContent = lessContent.Replace(GetStyleBloque("lessParam", lessContent), Environment.NewLine + newParamBlock);
            lessContent = lessContent.Replace(GetStyleBloque("gf", lessContent), Environment.NewLine + newGfBlock);

            // Save less file
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(newResultLessPath))
            {
                file.Write(lessContent);
            }

            // Compile the Less file
            string compiledStyle = GetCssFromLessString(lessContent, false, true, true).Replace("@import", "@IMPORT");

            // Save compiled file
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(newResultCssPath))
            {
                file.Write(compiledStyle);
            }

            var resp = new HttpResponseMessage()
            {
                Content = new StringContent("ok")
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return resp;
        }

        [HttpGet]
        public HttpResponseMessage Delete()
        {

            var pageId = HttpContext.Current.Request["pageId"];

            // Path to the less and css files
            string newResultLessPath = HttpContext.Current.Server.MapPath(string.Format(resultLessPath, pageId));
            string newResultCssPath = HttpContext.Current.Server.MapPath(string.Format(resultCssPath, pageId));

            // Delete all style file for this page
            System.IO.File.Delete(newResultLessPath);
            System.IO.File.Delete(newResultCssPath);

            var resp = new HttpResponseMessage()
            {
                Content = new StringContent("ok")
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return resp;

        }

        [HttpGet]
        public HttpResponseMessage GridStyle() {

            // Load current content
            int pageId = int.TryParse(HttpContext.Current.Request["pageId"], out pageId) ? pageId : -1;
            var contentService = Services.ContentService;
            IContent content = contentService.GetById(pageId);

            // Path to the new grid less
            string newResultGridLessPath = HttpContext.Current.Server.MapPath(string.Format(resultGridLessPath, pageId));

            // Load current grid Less
            string gridLessContent = string.Empty;
            using (System.IO.StreamReader sr = new System.IO.StreamReader(newResultGridLessPath))
            {
                gridLessContent = sr.ReadToEnd();
            }

            //Get parameter block
            string lessParam = GetStyleBloque("lessParam", gridLessContent);

            //Get style block
            string gridStyle = GetStyleBloque("gridStyle", gridLessContent);

            // Look after grid properies
            foreach (var property in content.Properties)
            {

                if (property.PropertyType.PropertyEditorAlias == "Umbraco.Grid")
                {
                    dynamic grid = Newtonsoft.Json.Linq.JObject.Parse(property.Value.ToString());
                    foreach (var column in grid.columns)
                    {
                        foreach (var row in column.rows)
                        {

                            string newTag = "grid-row-" + column.rows.IndexOf(row);

                            if (gridStyle.IndexOf(newTag + " ") < 0)
                            {
                                lessParam += string.Format("@backgroundRowColor__{0}:;\r\n" +
                                    "@backgroundRowImageOrPattern__{0}:;\r\n" +
                                    "@backgroundRowGradientColor__{0}:;\r\n" +
                                    "@backgroundRowPosition__{0}:;\r\n" +
                                    "@backgroundRowCover__{0}:;\r\n" +
                                    "@backgroundRowRepeat__{0}:;\r\n" +
                                    "@backgroundRowAttachment__{0}:;\r\n", newTag);

                                gridStyle += string.Format("\r\n\r\n.Row-cover__{0} () when (@backgroundRowCover__{0} = true) {{ \r\n" +
                                    "-webkit-background-size: cover; \r\n" +
                                    "-moz-background-size: cover; \r\n" +
                                    "-o-background-size: cover; \r\n" +
                                    "background-size: cover; \r\n" +
                                    "}} \r\n\r\n" +
                                    ".{0} {{ \r\n" +
                                    "background-color:@backgroundRowColor__{0}; \r\n" +
                                    "background: -moz-linear-gradient(top, @backgroundRowColor__{0} 41%,@backgroundRowGradientColor__{0} 100%); \r\n" +
                                    "background: -webkit-gradient(linear, left top, left bottom, color-stop(41%,@backgroundRowColor__{0}), color-stop(100%,@backgroundRowGradientColor__{0})); \r\n" +
                                    "background: -webkit-linear-gradient(top, @backgroundRowColor__{0} 41%,@backgroundRowGradientColor__{0} 100%); \r\n" +
                                    "background: -o-linear-gradient(top, @backgroundRowColor__{0} 41%,@backgroundRowGradientColor__{0} 100%); \r\n" +
                                    "background: -ms-linear-gradient(top, @backgroundRowColor__{0} 41%,@backgroundRowGradientColor__{0} 100%); \r\n" +
                                    "background: linear-gradient(to bottom, @backgroundRowColor__{0} 41%,@backgroundRowGradientColor__{0} 100%); \r\n" +
                                    "background-image: @backgroundRowImageOrPattern__{0}; \r\n" +
                                    "background-position: @backgroundRowPosition__{0}; \r\n" +
                                    "background-repeat: @backgroundRowRepeat__{0}; \r\n" +
                                    "background-attachment: @backgroundRowAttachment__{0}; \r\n" +
                                    ".Row-cover__{0};}}", newTag);
                            }
                        }
                    }
                }

            }

            string result = "/***start-lessParam***/\r\n{0}/***end-lessParam***/\r\n\r\n/***start-gridStyle***/{1}/***end-gridStyle***/";
            result = string.Format(result, lessParam, gridStyle);

            // Save compiled file
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(newResultGridLessPath))
            {
                file.Write(result);
            }

            
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(result, Encoding.UTF8, "text/css")
            };

        }

        private static string GetCssFromLessString(string css, Boolean cacheEnabled, Boolean minifyOutput, Boolean disableVariableRedefines)
        {
            var config = dotless.Core.configuration.DotlessConfiguration.GetDefaultWeb();
            config.DisableVariableRedefines = disableVariableRedefines;
            config.CacheEnabled = cacheEnabled;
            config.MinifyOutput = minifyOutput;
            return dotless.Core.LessWeb.Parse(css, config);
        }

        private static string GetStyleBloque(string tag, string input)
        {
            string startTag = string.Format("/***start-{0}***/", tag);
            string endTag = string.Format("/***end-{0}***/", tag);

            int indexStartTag = input.IndexOf(startTag);
            int indexEndTag = input.IndexOf(endTag);

            if (indexStartTag >= 0 && indexEndTag >= 0)
                return input.Substring(indexStartTag, indexEndTag - indexStartTag).Replace(startTag, "").Replace(endTag, "");
            else
                return "";
        }

    }

}