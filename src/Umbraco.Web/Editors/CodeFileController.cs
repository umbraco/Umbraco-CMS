using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.Editors
{
    [PluginController("UmbracoApi")]
    [PrefixlessBodyModelValidator]
    [UmbracoApplicationAuthorizeAttribute(Core.Constants.Applications.Settings)]
    public class CodeFileController : BackOfficeNotificationsController
    {

        [ValidationFilter]
        public HttpResponseMessage PostCreate(string type, CodeFileDisplay display)
        {
            switch (type)
            {
                case Core.Constants.Trees.PartialViews:
                    var view = new PartialView(display.VirtualPath);
                    var result = Services.FileService.CreatePartialView(view, display.Snippet, Security.CurrentUser.Id);
                    return result.Success == true ? Request.CreateResponse(HttpStatusCode.OK) : Request.CreateNotificationValidationErrorResponse(result.Exception.Message);
                case Core.Constants.Trees.PartialViewMacros:
                    var viewMacro = new PartialView(display.VirtualPath);
                    var resultMacro = Services.FileService.CreatePartialViewMacro(viewMacro, display.Snippet, Security.CurrentUser.Id);
                    return resultMacro.Success == true ? Request.CreateResponse(HttpStatusCode.OK) : Request.CreateNotificationValidationErrorResponse(resultMacro.Exception.Message);
                case Core.Constants.Trees.Scripts:
                    var script = new Script(display.VirtualPath);
                    Services.FileService.SaveScript(script, Security.CurrentUser.Id);
                    return Request.CreateResponse(HttpStatusCode.OK);
                default:
                    return Request.CreateResponse(HttpStatusCode.NotFound);
            }
        }


        public CodeFileDisplay GetByPath(string type, string virtualPath)
        {
            if (string.IsNullOrWhiteSpace(type) == false && string.IsNullOrWhiteSpace(virtualPath) == false)
            {
                if (type == Core.Constants.Trees.PartialViews)
                {
                    var view = Services.FileService.GetPartialView(virtualPath);
                    if (view != null)
                    {
                        var display = Mapper.Map<IPartialView, CodeFileDisplay>(view);
                        display.FileType = Core.Constants.Trees.PartialViews;
                        return display;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (type == Core.Constants.Trees.PartialViewMacros)
                {
                    var viewMacro = Services.FileService.GetPartialViewMacro(virtualPath);
                    if (viewMacro != null)
                    {
                        var display = Mapper.Map<IPartialView, CodeFileDisplay>(viewMacro);
                        display.FileType = Core.Constants.Trees.PartialViewMacros;
                        return display;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (type == Core.Constants.Trees.Scripts)
                {
                    var script = Services.FileService.GetScriptByName(virtualPath);
                    if (script != null)
                    {
                        var display = Mapper.Map<Script, CodeFileDisplay>(script);
                        display.FileType = Core.Constants.Trees.Scripts;
                        return display;
                    }
                    else
                    {
                        return null;
                    }

                }
                else
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }
            }
            else
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }

        [HttpDelete]
        [HttpPost]
        public HttpResponseMessage Delete(string type, string virtualPath)
        {
            if (string.IsNullOrWhiteSpace(type) == false && string.IsNullOrWhiteSpace(virtualPath) == false)
            {
                switch (type)
                {
                    case Core.Constants.Trees.PartialViews:
                        if (Services.FileService.DeletePartialView(virtualPath, Security.CurrentUser.Id))
                        {
                            return Request.CreateResponse(HttpStatusCode.OK);
                        }
                        else
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.NotFound, "No Partial View found with the specified path");
                        }
                        break;
                    case Core.Constants.Trees.PartialViewMacros:
                        if (Services.FileService.DeletePartialViewMacro(virtualPath, Security.CurrentUser.Id))
                        {
                            return Request.CreateResponse(HttpStatusCode.OK);
                        }
                        else
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.NotFound, "No Partial View Macro found with the specified path");
                        }
                        break;
                    case Core.Constants.Trees.Scripts:
                        if (Services.FileService.GetScriptByName(virtualPath) != null)
                        {
                            Services.FileService.DeleteScript(virtualPath, Security.CurrentUser.Id);
                            return Request.CreateResponse(HttpStatusCode.OK);
                        }
                        else
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.NotFound, "No Script found with the specified path");
                        }
                        break;
                    default:
                        return Request.CreateResponse(HttpStatusCode.NotFound);
                }
            }
            else
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }

        public CodeFileDisplay PostSave(CodeFileDisplay display)
        {
            if (ModelState.IsValid == false)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            }

            if (display == null && string.IsNullOrWhiteSpace(display.FileType) == true)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            else
            {
                if (display.FileType == Core.Constants.Trees.PartialViews)
                {
                    var view = Services.FileService.GetPartialView(display.VirtualPath);
                    if (view != null)
                    {
                        view.Content = display.Content;
                        view.Path = display.Name;
                        var result = Services.FileService.SavePartialView(view, Security.CurrentUser.Id);
                        if (result.Success == true)
                        {
                            return Mapper.Map(view, display);
                        } else
                        {
                            display.AddErrorNotification(
                                Services.TextService.Localize("speechBubbles/partialViewErrorHeader"),
                                Services.TextService.Localize("speechBubbles/partialViewErrorText"));
                        }
                    }
                    else
                    {
                        throw new HttpResponseException(HttpStatusCode.NotFound);
                    }
                }
                else if (display.FileType == Core.Constants.Trees.PartialViewMacros)
                {
                    var viewMacro = Services.FileService.GetPartialViewMacro(display.VirtualPath);
                    if (viewMacro != null)
                    {
                        viewMacro.Content = display.Content;
                        viewMacro.Path = display.Name;
                        var result = Services.FileService.SavePartialViewMacro(viewMacro, Security.CurrentUser.Id);
                        if (result.Success == false)
                        {
                            display.AddErrorNotification(
                                Services.TextService.Localize("speechBubbles/macroPartialViewErrorHeader"),
                                Services.TextService.Localize("speechBubbles/macroPartialViewErrorText"));
                        }
                    }
                    else
                    {
                        throw new HttpResponseException(HttpStatusCode.NotFound);
                    }
                }
                else if (display.FileType == Core.Constants.Trees.Scripts)
                {
                    var script = Services.FileService.GetScriptByName(display.VirtualPath);
                    if (script != null)
                    {
                        script.Content = display.Content;
                        script.Path = display.Name;
                        Services.FileService.SaveScript(script, Security.CurrentUser.Id);

                    }
                    else
                    {
                        throw new HttpResponseException(HttpStatusCode.NotFound);
                    }
                }
                else
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }
                return display;
            }
        }
    }
}
