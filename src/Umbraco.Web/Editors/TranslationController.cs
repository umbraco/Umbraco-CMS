using AutoMapper;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Xml.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;

using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    [PluginController("UmbracoApi")]
    [UmbracoApplicationAuthorize(Constants.Applications.Translation)]
    public class TranslationController : UmbracoAuthorizedJsonController
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public TranslationController()
                : this(UmbracoContext.Current)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="umbracoContext"></param>
        public TranslationController(UmbracoContext umbracoContext)
                : base(umbracoContext)
        {
        }

        public HttpResponseMessage PostCloseTask(int id)
        {
            var task = Services.TaskService.GetTaskById(id);

            if (task.AssigneeUserId != Security.CurrentUser.Id && task.OwnerUserId != Security.CurrentUser.Id)
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            task.Closed = true;

            Services.TaskService.Save(task);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TaskDisplay GetTaskById(int id)
        {
            var task = Services.TaskService.GetTaskById(id);

            if (task.AssigneeUserId != Security.CurrentUser.Id && task.OwnerUserId != Security.CurrentUser.Id)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
            }

            var document = Services.ContentService.GetById(task.EntityId);
            var assignedBy = Services.UserService.GetUserById(task.OwnerUserId);
            var assignedTo = Services.UserService.GetUserById(task.AssigneeUserId);

            var t = new TaskDisplay
            {
                CreatedDate = task.CreateDate,
                Closed = task.Closed,
                AssignedBy = Mapper.Map<IUser, UserDisplay>(assignedBy),
                AssignedTo = Mapper.Map<IUser, UserDisplay>(assignedTo),
                Comment = task.Comment,
                TotalWords = CountWords(document),
                Properties = new List<PropertyDisplay>
                {
                    new PropertyDisplay { Name = Services.TextService.Localize("nodeName"), Value = document.Name }
                }
            };

            foreach (var prop in document.Properties)
            {
                if (prop.Value is string asString)
                {
                    t.Properties.Add(new PropertyDisplay { Name = prop.Alias, Value = asString });
                }
            }

            return t;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public HttpResponseMessage GetTaskXml(int id)
        {
            var task = Services.TaskService.GetTaskById(id);
            var document = Services.ContentService.GetById(task.EntityId);
            // [SEB] Move this in the TaskService
            var xml = new XDocument(new XElement("tasks"));

            var taskElement = new XElement("task",
                new XAttribute("Id", id),
                new XAttribute("Date", task.CreateDate.ToString("o")),
                new XAttribute("NodeId", task.EntityId),
                new XAttribute("TotalWords", CountWords(document)));

            taskElement.Add(new XElement("Comment", task.Comment));
            // [SEB] Url to preview
            taskElement.Add(new XElement("PreviewUrl", "URL to preview"));

            taskElement.Add(document.ToXml(Services.PackagingService, true));

            xml.Root.Add(taskElement);
            
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(xml.ToStringWithPrologue(SaveOptions.DisableFormatting), Encoding.UTF8, "application/json");

            return response;
        }

        private int CountWords(IContent document)
        {
            int words = CountWordsInString(document.Name);

            foreach (Property p in document.Properties)
            {
                if (p.Value is string asString)
                {
                    var trimmed = asString.Trim();

                    if (trimmed.IsNullOrWhiteSpace() == false)
                    {
                        words += CountWordsInString(trimmed);
                    }
                }
            }

            return words;
        }

        private int CountWordsInString(string Text)
        {
            string pattern = @"<(.|\n)*?>";
            string tmpStr = Regex.Replace(Text, pattern, string.Empty);

            tmpStr = tmpStr.Replace("\t", " ").Trim();
            tmpStr = tmpStr.Replace("\n", " ");
            tmpStr = tmpStr.Replace("\r", " ");

            MatchCollection collection = Regex.Matches(tmpStr, @"[\S]+");

            return collection.Count;
        }

    }
}
