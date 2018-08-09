using AutoMapper;
using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Xml.Linq;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;

using Constants = Umbraco.Core.Constants;
using Umbraco.Core.Logging;
using System.Net.Mime;

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

        // [SEB] Rename in PUT
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
        /// <returns></returns>
        public IEnumerable<TaskDisplay> GetAllTaskAssignedToCurrentUser()
        {
            return Services
                .TaskService
                .GetTasks(assignedUser: Security.CurrentUser.Id)
                .Select(t => ConvertTask(t));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TaskDisplay> GetAllTaskCreatedByCurrentUser()
        {
            return Services
                .TaskService
                .GetTasks(ownerUser: Security.CurrentUser.Id)
                .Select(t => ConvertTask(t));
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

            return ConvertTask(task);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public HttpResponseMessage GetTaskXml(int id)
        {
            var task = Services.TaskService.GetTaskById(id);
            var xml = new XDocument(new XElement("tasks", GetTaskXElement(task)));

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(xml.ToStringWithPrologue(SaveOptions.None), Encoding.UTF8, "application/json");

            return response;
        }

        // [SEB] Use that one instead of the one above
        public HttpResponseMessage GetTasksXml(string ids)
        {
            var xml = new XDocument(new XElement("tasks"));

            foreach (var id in ids.Split(',').Select(i => int.Parse(i)))
            {
                var task = Services.TaskService.GetTaskById(id);

                xml.Root.Add(GetTaskXElement(task));
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(xml.ToStringWithPrologue(SaveOptions.None), Encoding.UTF8, "application/json");

            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        // [SEB] I don't use FileName & Id, remove them?
        public List<ImportTaskResult> PutSubmitTasks(TaskFile data)
        {
            var match = Regex.Match(data.Content, @"data:(?<type>.+?);base64,(?<data>.+)");
            var base64Data = match.Groups["data"].Value;
            var contentType = match.Groups["type"].Value;
            var result = new List<ImportTaskResult>();

            if (contentType == MediaTypeNames.Text.Xml)
            {
                var xml = XDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(base64Data)));

                var tasksXml = xml.Root.Elements("task");

                foreach (var taskXml in tasksXml)
                {
                    var entityXml = taskXml.Elements().FirstOrDefault(e => UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema ? e.Name.LocalName.InvariantEquals("node") : e.Attribute("isDoc") != null);

                    Task task = Services.TaskService.GetTaskById(int.Parse(taskXml.Attribute("Id").Value));

                    // [SEB] Use a Nullable Int for EntityId?
                    if (task != null && (data.NodeId == -1 || task.EntityId == data.NodeId) && (task.AssigneeUserId == Security.CurrentUser.Id || task.OwnerUserId == Security.CurrentUser.Id))
                    {
                        result.Add(new ImportTaskResult { TaskId = task.Id, EntityId = ImportTask(entityXml) });

                        // [SEB] Shouldn't we close all the task ?
                        task.Closed = true;
                        //Services.TaskService.Save(task);
                    }
                    else
                    {
                        result.Add(new ImportTaskResult { TaskId = int.Parse(taskXml.Attribute("Id").Value) });
                    }
                }
            }
            else
            {
                // [SEB] Handle ZIP files
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Text"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="creator"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        private int ImportTask(XElement source, IUser creator = null)
        {
            // [SEB][ASK] Put that in the ContentService ?
            creator = creator ?? Security.CurrentUser;

            bool isLegacy = UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema;
            int id = int.Parse(source.Attribute("id").Value);

            var document = Services.ContentService.GetById(id);

            if (document == null || document.ParentId != int.Parse(source.Attribute("parentID").Value))
            {
                string nodeTypeAlias = isLegacy ? source.Attribute("nodeTypeAlias").Value : source.Name.LocalName;

                Services.ContentService.CreateContent(
                    source.Attribute("nodeName").Value,
                    int.Parse(source.Attribute("parentID").Value),
                    nodeTypeAlias,
                    Security.CurrentUser.Id);
            }
            else
            {
                document.Name = source.Attribute("nodeName").Value;
            }

            document.CreateDate = DateTime.Parse(source.Attribute("createDate").Value);

            var properties = source.Elements().Where(e => isLegacy ? e.Name.LocalName.InvariantEquals("data") : e.Attribute("isDoc") == null);

            foreach (var propertyXml in properties)
            {
                var alias = isLegacy ? propertyXml.Attribute("alias").Value : propertyXml.Name.LocalName;
                var property = document.Properties[alias];
                var value = XmlHelper.GetNodeValue(propertyXml);

                if (property != null)
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        var prevals = Services.DataTypeService.GetPreValuesCollectionByDataTypeId(property.PropertyType.DataTypeDefinitionId);

                        if (prevals != null && prevals.PreValuesAsDictionary.Count != 0)
                        {
                            var list = new List<string>(value.Split(','));

                            foreach (var preval in prevals.PreValuesAsDictionary)
                            {
                                string pval = preval.Value.Value;
                                string pid = preval.Value.Id.ToString();

                                if (list.Contains(pval))
                                {
                                    list[list.IndexOf(pval)] = pid;
                                }
                            }

                            property.Value = string.Join(",", list);
                        }
                        else
                        {
                            property.Value = value;
                        }
                    }
                }
                else
                {
                    LogHelper.Warn<IContent>(string.Format("Couldn't import property '{0}' as the property type doesn't exist on this document type", alias));
                }
            }

            Services.ContentService.Save(document);

            // [SEB][ASK] is that really necessary as it seems that task are not nested? Legacy ?
            var subTasks = source.Elements().Where(e => isLegacy ? e.Name.LocalName.InvariantEquals("node") : e.Attribute("isDoc") != null);

            foreach (var subTask in subTasks)
            {
                ImportTask(subTask, creator);
            }

            return document.Id;
        }

        // [SEB] Use AutoMapper here?
        private TaskDisplay ConvertTask(Task task)
        {
            var document = Services.ContentService.GetById(task.EntityId);
            var assignedBy = Services.UserService.GetUserById(task.OwnerUserId);
            var assignedTo = Services.UserService.GetUserById(task.AssigneeUserId);

            var t = new TaskDisplay
            {
                Id = task.Id,
                CreatedDate = task.CreateDate,
                Closed = task.Closed,
                AssignedBy = Mapper.Map<IUser, UserDisplay>(assignedBy),
                AssignedTo = Mapper.Map<IUser, UserDisplay>(assignedTo),
                Comment = task.Comment,
                NodeId = document.Id,
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

        // [SEB] Move this in the TaskService
        private XElement GetTaskXElement(Task task)
        {
            var document = Services.ContentService.GetById(task.EntityId);

            var taskElement = new XElement("task",
                new XAttribute("Id", task.Id),
                new XAttribute("Date", task.CreateDate.ToString("o")),
                new XAttribute("NodeId", task.EntityId),
                new XAttribute("TotalWords", CountWords(document)));

            taskElement.Add(new XElement("Comment", task.Comment));
            // [SEB] Url to preview
            taskElement.Add(new XElement("PreviewUrl", "URL to preview"));

            taskElement.Add(document.ToXml(Services.PackagingService, true));

            return taskElement;
        }
    }
}
