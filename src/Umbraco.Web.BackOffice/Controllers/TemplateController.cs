using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

[PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
[Authorize(Policy = AuthorizationPolicies.TreeAccessTemplates)]
[ParameterSwapControllerActionSelector(nameof(GetById), "id", typeof(int), typeof(Guid), typeof(Udi))]
public class TemplateController : BackOfficeNotificationsController
{
    private readonly IDefaultViewContentProvider _defaultViewContentProvider;
    private readonly ITemplateService _templateService;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly IUmbracoMapper _umbracoMapper;

    [ActivatorUtilitiesConstructor]
    public TemplateController(
        ITemplateService templateService,
        IUmbracoMapper umbracoMapper,
        IShortStringHelper shortStringHelper,
        IDefaultViewContentProvider defaultViewContentProvider,
        IFileService fileService)
    {
        _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
        _umbracoMapper = umbracoMapper ?? throw new ArgumentNullException(nameof(umbracoMapper));
        _shortStringHelper = shortStringHelper ?? throw new ArgumentNullException(nameof(shortStringHelper));
        _defaultViewContentProvider = defaultViewContentProvider ??
                                      throw new ArgumentNullException(nameof(defaultViewContentProvider));
    }

    /// <summary>
    /// Gets data type by alias
    /// </summary>
    /// <param name="alias"></param>
    /// <returns></returns>
    public TemplateDisplay? GetByAlias(string alias)
    {
        ITemplate? template = _templateService.GetAsync(alias).GetAwaiter().GetResult();
        return template == null ? null : _umbracoMapper.Map<ITemplate, TemplateDisplay>(template);
    }

    /// <summary>
    ///     Get all templates
    /// </summary>
    /// <returns></returns>
    public IEnumerable<EntityBasic>? GetAll() => _templateService.GetAllAsync().GetAwaiter().GetResult()
        ?.Select(_umbracoMapper.Map<ITemplate, EntityBasic>).WhereNotNull();

    /// <summary>
    ///     Gets the template json for the template id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ActionResult<TemplateDisplay?> GetById(int id)
    {
        ITemplate? template = _templateService.GetAsync(id).GetAwaiter().GetResult();
        if (template == null)
        {
            return NotFound();
        }

        return _umbracoMapper.Map<ITemplate, TemplateDisplay>(template);
    }


    /// <summary>
    ///     Gets the template json for the template guid
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ActionResult<TemplateDisplay?> GetById(Guid id)
    {
        ITemplate? template = _templateService.GetAsync(id).GetAwaiter().GetResult();
        if (template == null)
        {
            return NotFound();
        }

        return _umbracoMapper.Map<ITemplate, TemplateDisplay>(template);
    }

    /// <summary>
    ///     Gets the template json for the template udi
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ActionResult<TemplateDisplay?> GetById(Udi id)
    {
        var guidUdi = id as GuidUdi;
        if (guidUdi == null)
        {
            return NotFound();
        }

        ITemplate? template = _templateService.GetAsync(guidUdi.Guid).GetAwaiter().GetResult();
        if (template == null)
        {
            return NotFound();
        }

        return _umbracoMapper.Map<ITemplate, TemplateDisplay>(template);
    }

    /// <summary>
    ///     Deletes a template with a given ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete]
    [HttpPost]
    public IActionResult DeleteById(int id)
    {
        ITemplate? template = _templateService.GetAsync(id).GetAwaiter().GetResult();
        if (template == null)
        {
            return NotFound();
        }

        _templateService.DeleteAsync(template.Alias, Constants.Security.SuperUserKey).GetAwaiter().GetResult();
        return Ok();
    }

    public TemplateDisplay? GetScaffold(int id)
    {
        //empty default
        var dt = new Template(_shortStringHelper, string.Empty, string.Empty)
        {
            Path = "-1"
        };

        if (id > 0)
        {
            ITemplate? master = _templateService.GetAsync(id).GetAwaiter().GetResult();
            if (master != null)
            {
                dt.SetMasterTemplate(master);
            }
        }

        var content = _defaultViewContentProvider.GetDefaultFileContent(dt.MasterTemplateAlias);
        TemplateDisplay? scaffold = _umbracoMapper.Map<ITemplate, TemplateDisplay>(dt);

        if (scaffold is not null)
        {
            scaffold.Content = content;
        }

        return scaffold;
    }

    /// <summary>
    ///     Saves the data type
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
            ITemplate? template = _templateService.GetAsync(display.Id).GetAwaiter().GetResult();
            if (template == null)
            {
                return NotFound();
            }

            var changeAlias = template.Alias != display.Alias;

            _umbracoMapper.Map(display, template);

            _templateService.UpdateAsync(template, Constants.Security.SuperUserKey).GetAwaiter().GetResult();

            if (changeAlias)
            {
                template = _templateService.GetAsync(template.Id).GetAwaiter().GetResult();
            }

            _umbracoMapper.Map(template, display);
        }
        else
        {
            //create
            ITemplate? master = null;
            if (string.IsNullOrEmpty(display.MasterTemplateAlias) == false)
            {
                master = _templateService.GetAsync(display.MasterTemplateAlias).GetAwaiter().GetResult();
                if (master == null)
                {
                    return NotFound();
                }
            }

            // we need to pass the template name as alias to keep the template file casing consistent with templates created with content
            // - see comment in FileService.CreateTemplateForContentType for additional details
            Attempt<ITemplate, TemplateOperationStatus> result =
                _templateService.CreateAsync(display.Name!, display.Name!, display.Content, Constants.Security.SuperUserKey).GetAwaiter().GetResult();
            if (result.Success == false)
            {
                return NotFound();
            }

            _umbracoMapper.Map(result.Result, display);
        }

        return display;
    }
}
