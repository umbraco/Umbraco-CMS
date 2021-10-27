using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Editors
{
    [PluginController("UmbracoApi")]
    [UmbracoTreeAuthorize(Constants.Trees.Templates)]
    [TemplateControllerConfiguration]
    public class TemplateController : BackOfficeNotificationsController
    {
        public TemplateController(IGlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor, ISqlContext sqlContext, ServiceContext services, AppCaches appCaches, IProfilingLogger logger, IRuntimeState runtimeState, UmbracoHelper umbracoHelper)
            : base(globalSettings, umbracoContextAccessor, sqlContext, services, appCaches, logger, runtimeState, umbracoHelper)
        {
        }

        /// <summary>
        /// Configures this controller with a custom action selector
        /// </summary>
        private class TemplateControllerConfigurationAttribute : Attribute, IControllerConfiguration
        {
            public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
            {
                controllerSettings.Services.Replace(typeof(IHttpActionSelector), new ParameterSwapControllerActionSelector(
                    new ParameterSwapControllerActionSelector.ParameterSwapInfo("GetById", "id", typeof(int), typeof(Guid), typeof(Udi))
                ));
            }
        }

        /// <summary>
        /// Gets data type by alias
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        public TemplateDisplay GetByAlias(string alias)
        {
            var template = Services.FileService.GetTemplate(alias);
            return template == null ? null : Mapper.Map<ITemplate, TemplateDisplay>(template);
        }

        /// <summary>
        /// Get all templates
        /// </summary>
        /// <returns></returns>
        public IEnumerable<EntityBasic> GetAll()
        {
            return Services.FileService.GetTemplates().Select(Mapper.Map<ITemplate, EntityBasic>);
        }

        /// <summary>
        /// Gets the template json for the template id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TemplateDisplay GetById(int id)
        {
            var template = Services.FileService.GetTemplate(id);
            if (template == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            return Mapper.Map<ITemplate, TemplateDisplay>(template);
        }

        /// <summary>
        /// Gets the template json for the template guid
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TemplateDisplay GetById(Guid id)
        {
            var template = Services.FileService.GetTemplate(id);
            if (template == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            return Mapper.Map<ITemplate, TemplateDisplay>(template);
        }

        /// <summary>
        /// Gets the template json for the template udi
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TemplateDisplay GetById(Udi id)
        {
            var guidUdi = id as GuidUdi;
            if (guidUdi == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var template = Services.FileService.GetTemplate(guidUdi.Guid);
            if (template == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return Mapper.Map<ITemplate, TemplateDisplay>(template);
        }

        /// <summary>
        /// Deletes a template with a given ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [HttpPost]
        public HttpResponseMessage DeleteById(int id)
        {
            var template = Services.FileService.GetTemplate(id);
            if (template == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            Services.FileService.DeleteTemplate(template.Alias);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        public TemplateDisplay GetScaffold(int id)
        {
            //empty default
            var dt = new Template("", "");
            dt.Path = "-1";

            if (id > 0)
            {
                var master = Services.FileService.GetTemplate(id);
                if(master != null)
                {
                    dt.SetMasterTemplate(master);
                }
            }

            var content = ViewHelper.GetDefaultFileContent( layoutPageAlias: dt.MasterTemplateAlias );
            var scaffold = Mapper.Map<ITemplate, TemplateDisplay>(dt);

            scaffold.Content =  content + "\r\n\r\n@* the fun starts here *@\r\n\r\n";
            return scaffold;
        }

        /// <summary>
        /// Saves the data type
        /// </summary>
        /// <param name="display"></param>
        /// <returns></returns>
        public TemplateDisplay PostSave(TemplateDisplay display)
        {

            //Checking the submitted is valid with the Required attributes decorated on the ViewModel
            if (ModelState.IsValid == false)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            }

            if (display.Id > 0)
            {
                // update
                var template = Services.FileService.GetTemplate(display.Id);
                if (template == null)
                    throw new HttpResponseException(HttpStatusCode.NotFound);

                var changeMaster = template.MasterTemplateAlias != display.MasterTemplateAlias;
                var changeAlias = template.Alias != display.Alias;

                Mapper.Map(display, template);

                if (changeMaster)
                {
                    if (string.IsNullOrEmpty(display.MasterTemplateAlias) == false)
                    {

                        var master = Services.FileService.GetTemplate(display.MasterTemplateAlias);
                        if(master == null || master.Id == display.Id)
                        {
                            template.SetMasterTemplate(null);
                        }else
                        {
                            template.SetMasterTemplate(master);

                            //After updating the master - ensure we update the path property if it has any children already assigned
                            var templateHasChildren = Services.FileService.GetTemplateDescendants(display.Id);

                            foreach (var childTemplate in templateHasChildren)
                            {
                                //template ID to find
                                var templateIdInPath = "," + display.Id + ",";

                                if (string.IsNullOrEmpty(childTemplate.Path))
                                {
                                    continue;
                                }

                                //Find position in current comma separate string path (so we get the correct children path)
                                var positionInPath = childTemplate.Path.IndexOf(templateIdInPath) + templateIdInPath.Length;

                                //Get the substring of the child & any children (descendants it may have too)
                                var childTemplatePath = childTemplate.Path.Substring(positionInPath);

                                //As we are updating the template to be a child of a master
                                //Set the path to the master's path + its current template id + the current child path substring
                                childTemplate.Path = master.Path + "," + display.Id + "," + childTemplatePath;

                                //Save the children with the updated path
                                Services.FileService.SaveTemplate(childTemplate);
                            }
                        }
                    }
                    else
                    {
                        //remove the master
                        template.SetMasterTemplate(null);
                    }
                }

                Services.FileService.SaveTemplate(template);

                if (changeAlias)
                {
                    template = Services.FileService.GetTemplate(template.Id);
                }

                Mapper.Map(template, display);
            }
            else
            {
                //create
                ITemplate master = null;
                if (string.IsNullOrEmpty(display.MasterTemplateAlias) == false)
                {
                    master = Services.FileService.GetTemplate(display.MasterTemplateAlias);
                    if (master == null)
                        throw new HttpResponseException(HttpStatusCode.NotFound);
                }

                // we need to pass the template name as alias to keep the template file casing consistent with templates created with content
                // - see comment in FileService.CreateTemplateForContentType for additional details
                var template = Services.FileService.CreateTemplateWithIdentity(display.Name, display.Name, display.Content, master);
                Mapper.Map(template, display);
            }

            return display;
        }
    }
}
