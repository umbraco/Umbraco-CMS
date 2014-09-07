using AutoMapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.Editors
{
    [PluginController("UmbracoApi")]
    [UmbracoTreeAuthorize(Core.Constants.Trees.Templates)]
    public class TemplateController : UmbracoAuthorizedJsonController
    {
        /// <summary>
        /// Gets the content json for the content id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TemplateDisplay GetById(int id)
        {
            var template = Services.FileService.GetTemplate(id);
            if (template == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            TemplateDisplay t = new TemplateDisplay();
            t.Alias = template.Alias;
            t.Content = template.Content;
            t.Id = template.Id;
            t.Name = template.Name;

            return t;
        }

        /// <summary>
        /// Deletes a data type wth a given ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [HttpPost]
        public HttpResponseMessage DeleteById(int id)
        {
            var foundTemplate = Services.FileService.GetTemplate(id);
            if (foundTemplate == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            Services.FileService.DeleteTemplate(foundTemplate.Alias);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPost]
        public HttpResponseMessage PostSaveAndRender(dynamic model)
        {
            var foundTemplate = Services.FileService.GetTemplate((int)model.templateId);
            if (foundTemplate == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            foundTemplate.Content = model.html;
            Services.FileService.SaveTemplate(foundTemplate);

            string result = string.Empty;
            try
            {
                var url = "http://" + Request.RequestUri.Host + "/" + model.pageId + ".aspx?altTemplate=" + foundTemplate.Alias;
                result = url;

                WebClient wc = new WebClient();
                result = wc.DownloadString(new Uri(url));
            }
            catch (WebException exception)
            {
                if (exception.Response != null)
                {
                    var responseStream = exception.Response.GetResponseStream();
                    
                    if (responseStream != null)
                    {
                        using (var reader = new StreamReader(responseStream))
                        {
                            result = reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result += ex.ToString();
            }
            
                        
            return new HttpResponseMessage()
            {
                Content = new StringContent(
                    result,
                    Encoding.UTF8,
                    "text/html"
                )
            };
        }


       
    }
}
