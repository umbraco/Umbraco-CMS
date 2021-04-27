using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Web.BackOffice.Controllers
{
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
    [Authorize(Policy = AuthorizationPolicies.TreeAccessTemplates)]
    [ParameterSwapControllerActionSelector(nameof(GetById), "id", typeof(int), typeof(Guid), typeof(Udi))]
    public class TemplateController : BackOfficeNotificationsController
    {
        private readonly IFileService _fileService;
        private readonly IUmbracoMapper _umbracoMapper;
        private readonly IShortStringHelper _shortStringHelper;

        public TemplateController(
            IFileService fileService,
            IUmbracoMapper umbracoMapper,
            IShortStringHelper shortStringHelper)
        {
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _umbracoMapper = umbracoMapper ?? throw new ArgumentNullException(nameof(umbracoMapper));
            _shortStringHelper = shortStringHelper ?? throw new ArgumentNullException(nameof(shortStringHelper));
        }

        /// <summary>
        /// Gets data type by alias
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        public TemplateDisplay GetByAlias(string alias)
        {
            var template = _fileService.GetTemplate(alias);
            return template == null ? null : _umbracoMapper.Map<ITemplate, TemplateDisplay>(template);
        }

        /// <summary>
        /// Get all templates
        /// </summary>
        /// <returns></returns>
        public IEnumerable<EntityBasic> GetAll()
        {
            return _fileService.GetTemplates().Select(_umbracoMapper.Map<ITemplate, EntityBasic>);
        }

        /// <summary>
        /// Gets the template json for the template id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult<TemplateDisplay> GetById(int id)
        {
            var template = _fileService.GetTemplate(id);
            if (template == null)
                return NotFound();

            return _umbracoMapper.Map<ITemplate, TemplateDisplay>(template);
        }


        /// <summary>
        /// Gets the template json for the template guid
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult<TemplateDisplay> GetById(Guid id)
        {
            var template = _fileService.GetTemplate(id);
            if (template == null)
                return NotFound();

            return _umbracoMapper.Map<ITemplate, TemplateDisplay>(template);
        }

        /// <summary>
        /// Gets the template json for the template udi
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult<TemplateDisplay> GetById(Udi id)
        {
            var guidUdi = id as GuidUdi;
            if (guidUdi == null)
                return NotFound();

            var template = _fileService.GetTemplate(guidUdi.Guid);
            if (template == null)
            {
                return NotFound();
            }

            return _umbracoMapper.Map<ITemplate, TemplateDisplay>(template);
        }

        /// <summary>
        /// Deletes a template with a given ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [HttpPost]
        public IActionResult DeleteById(int id)
        {
            var template = _fileService.GetTemplate(id);
            if (template == null)
                return NotFound();

            _fileService.DeleteTemplate(template.Alias);
            return Ok();
        }

        public TemplateDisplay GetScaffold(int id)
        {
            //empty default
            var dt = new Template(_shortStringHelper, string.Empty, string.Empty);
            dt.Path = "-1";

            if (id > 0)
            {
                var master = _fileService.GetTemplate(id);
                if(master != null)
                {
                    dt.SetMasterTemplate(master);
                }
            }

            var content = ViewHelper.GetDefaultFileContent( layoutPageAlias: dt.MasterTemplateAlias );
            var scaffold = _umbracoMapper.Map<ITemplate, TemplateDisplay>(dt);

            scaffold.Content =  content + "\r\n\r\n@* the fun starts here *@\r\n\r\n";
            return scaffold;
        }

        /// <summary>
        /// Saves the data type
        /// </summary>
        /// <param name="display"></param>
        /// <returns></returns>
        public ActionResult<TemplateDisplay> PostSave(TemplateDisplay display)
        {

            //Checking the submitted is valid with the Required attributes decorated on the ViewModel
            if (ModelState.IsValid == false)
            {
                return ValidationProblem(ModelState);
            }

            if (display.Id > 0)
            {
                // update
                var template = _fileService.GetTemplate(display.Id);
                if (template == null)
                    return NotFound();

                var changeMaster = template.MasterTemplateAlias != display.MasterTemplateAlias;
                var changeAlias = template.Alias != display.Alias;

                _umbracoMapper.Map(display, template);

                if (changeMaster)
                {
                    if (string.IsNullOrEmpty(display.MasterTemplateAlias) == false)
                    {

                        var master = _fileService.GetTemplate(display.MasterTemplateAlias);
                        if(master == null || master.Id == display.Id)
                        {
                            template.SetMasterTemplate(null);
                        }else
                        {
                            template.SetMasterTemplate(master);

                            //After updating the master - ensure we update the path property if it has any children already assigned
                            var templateHasChildren = _fileService.GetTemplateDescendants(display.Id);

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
                                _fileService.SaveTemplate(childTemplate);
                            }
                        }
                    }
                    else
                    {
                        //remove the master
                        template.SetMasterTemplate(null);
                    }
                }

                _fileService.SaveTemplate(template);

                if (changeAlias)
                {
                    template = _fileService.GetTemplate(template.Id);
                }

                _umbracoMapper.Map(template, display);
            }
            else
            {
                //create
                ITemplate master = null;
                if (string.IsNullOrEmpty(display.MasterTemplateAlias) == false)
                {
                    master = _fileService.GetTemplate(display.MasterTemplateAlias);
                    if (master == null)
                        return NotFound();
                }

                // we need to pass the template name as alias to keep the template file casing consistent with templates created with content
                // - see comment in FileService.CreateTemplateForContentType for additional details
                var template = _fileService.CreateTemplateWithIdentity(display.Name, display.Name, display.Content, master);
                _umbracoMapper.Map(template, display);
            }

            return display;
        }
    }
}
