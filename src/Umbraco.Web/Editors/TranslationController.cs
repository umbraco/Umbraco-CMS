using AutoMapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Xml.Linq;
using umbraco.cms.businesslogic.utilities;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;

using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// The API controller used for translating content
    /// </summary>
    /// <remarks>
    /// This controller is decorated with the UmbracoApplicationAuthorizeAttribute which means that any user requesting
    /// access to ALL of the methods on this controller will need access to the translation application.
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

        /// <summary>
        /// Close a task
        /// </summary>
        /// <param name="id">ID of the task to close</param>
        /// <returns></returns>
        public HttpResponseMessage PutCloseTask(int id)
        {
            var task = Services.TaskService.GetTaskById(id);

            if (task == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            if (task.AssigneeUserId != Security.CurrentUser.Id && task.OwnerUserId != Security.CurrentUser.Id)
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            task.Closed = true;

            Services.TaskService.Save(task);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        /// <summary>
        /// Returns all the translation tasks assigned to the current user
        /// </summary>
        /// <returns>The translation tasks assigned to the current user</returns>
        public IEnumerable<TaskDisplay> GetAllTaskAssignedToCurrentUser()
        {
            return Services
                .TaskService
                .GetTasks(assignedUser: Security.CurrentUser.Id, taskTypeAlias: "toTranslate")
                .Select(t => Mapper.Map<Task, TaskDisplay>(t));
        }

        /// <summary>
        /// Returns all the translation tasks created by the current user
        /// </summary>
        /// <returns>The translation tasks created by the current user</returns>
        public IEnumerable<TaskDisplay> GetAllTaskCreatedByCurrentUser()
        {
            return Services
                .TaskService
                .GetTasks(ownerUser: Security.CurrentUser.Id, taskTypeAlias: "toTranslate")
                .Select(t => Mapper.Map<Task, TaskDisplay>(t));
        }

        /// <summary>
        /// Return a task based on its ID
        /// </summary>
        /// <param name="id">task ID</param>
        /// <returns></returns>
        public TaskDisplay GetTaskById(int id)
        {
            var task = Services.TaskService.GetTaskById(id);

            if (task == null || task.TaskType.Alias != "toTranslate" || (task.AssigneeUserId != Security.CurrentUser.Id && task.OwnerUserId != Security.CurrentUser.Id))
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return Mapper.Map<Task, TaskDisplay>(task);
        }

        /// <summary>
        /// Generate the XML for one or several tasks
        /// </summary>
        /// <param name="ids">Comma-separated list of task IDs</param>
        /// <returns>the XML for one or several tasks</returns>
        public HttpResponseMessage GetTasksXml(string ids)
        {
            var xml = new XDocument(new XElement("tasks"));
            var tasks = ids
                .Split(',')
                .Select(i => Services.TaskService.GetTaskById(int.Parse(i)))
                .Where(t => t != null);

            foreach (var task in tasks)
            {
                xml.Root.Add(Services.TaskService.GetTaskAsXml(task));
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(xml.ToStringWithPrologue(SaveOptions.None), Encoding.UTF8, "application/json");

            return response;
        }

        /// <summary>
        /// Submit a XML/ZIP document to validate translation
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public TranslationTaskImportSummary PutSubmitTasks(TaskFile data)
        {
            var result = new TranslationTaskImportSummary();
            
            var match = Regex.Match(data.Content, @"data:(?<type>.+?);base64,(?<data>.+)");
            var base64Data = match.Groups["data"].Value;
            var contentType = match.Groups["type"].Value;

            if (contentType == MediaTypeNames.Text.Xml)
            {
                result.Merge(ProcessXml(data, Encoding.UTF8.GetString(Convert.FromBase64String(base64Data))));
            }
            else if (contentType == "application/x-zip-compressed" || contentType == MediaTypeNames.Application.Zip)
            {
                string filename = IOHelper.MapPath(Path.Combine(SystemDirectories.Data, "translationFile_" + Guid.NewGuid().ToString() + ".zip"));
                string path = IOHelper.MapPath(Path.Combine(SystemDirectories.Data, "translationFiles_" + Guid.NewGuid().ToString()));

                System.IO.File.WriteAllBytes(filename, Convert.FromBase64String(base64Data));

                Zip.UnPack(filename, path, true);

                var xmlFiles = new DirectoryInfo(path).GetFiles("*.xml");

                if (xmlFiles.Length != 0)
                {
                    foreach (var file in xmlFiles)
                    {
                        string content = System.IO.File.ReadAllText(file.FullName);

                        result.Merge(ProcessXml(data, content));
                    }
                }

                Directory.Delete(path, true);
            }

            return result;
        }

        /// <summary>
        /// Process the XML content of a translation
        /// </summary>
        /// <param name="data"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        private TranslationTaskImportSummary ProcessXml(TaskFile data, string content)
        {
            var result = new TranslationTaskImportSummary();

            try
            {
                var xml = XDocument.Parse(content);

                var tasksXml = xml.Root.Elements("task");

                foreach (var taskXml in tasksXml)
                {
                    var entityXml = taskXml.Elements().FirstOrDefault(e => UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema ? e.Name.LocalName.InvariantEquals("node") : e.Attribute("isDoc") != null);

                    Task task = Services.TaskService.GetTaskById(int.Parse(taskXml.Attribute("Id").Value));

                    if (task != null && (data.EntityId == null || task.EntityId == data.EntityId) && (task.AssigneeUserId == Security.CurrentUser.Id || task.OwnerUserId == Security.CurrentUser.Id))
                    {
                        task.Closed = true;
                        //Services.TaskService.Save(task);

                        result.Outcome.Add(task.Id, Services.ContentService.ImportTaskXml(entityXml, Security.CurrentUser));
                        result.SuccessCount++;
                    }
                    else
                    {
                        result.ErrorCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                result.ErrorCount++;

                LogHelper.Error<TranslationController>(ex.Message, ex);
            }

            return result;
        }
    }
}
