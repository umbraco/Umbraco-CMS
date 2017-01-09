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
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Editors
{
    [PluginController("UmbracoApi")]
    public class CodeFileController : UmbracoAuthorizedJsonController
    {

        public HttpResponseMessage PostCreate(string type, CodeFileDisplay display)
        {
            switch (type)
            {
                case "partialView":
                    var view = new PartialView(display.VirtualPath);
                    var result = Services.FileService.CreatePartialView(view, display.Snippet, Security.CurrentUser.Id); 
                    return result.Success == true ? Request.CreateResponse(HttpStatusCode.OK, result.Result) : Request.CreateNotificationValidationErrorResponse(result.Exception.Message);
                case "partialViewMacro":
                    var viewMacro = new PartialView(display.VirtualPath);
                    var resultMacro = Services.FileService.CreatePartialViewMacro(viewMacro, display.Snippet, Security.CurrentUser.Id);
                    return resultMacro.Success == true ? Request.CreateResponse(HttpStatusCode.OK, resultMacro.Result) : Request.CreateNotificationValidationErrorResponse(resultMacro.Exception.Message);
                case "script":
                    var script = new Script(display.VirtualPath);
                    Services.FileService.SaveScript(script, Security.CurrentUser.Id);
                    return Request.CreateResponse(HttpStatusCode.OK);
                default:
                    throw new ArgumentException("File Type not supported", "type");
            }
        }


        public CodeFileDisplay GetByPath(string type, string virtualPath)
        {
            switch (type)
            {
                case "partialView":
                    var view = Services.FileService.GetPartialView(virtualPath);
                    return view == null ? null : Mapper.Map<IPartialView, CodeFileDisplay>(view);
                case "partialViewMacro":
                    var viewMacro = Services.FileService.GetPartialViewMacro(virtualPath);
                    return viewMacro == null ? null : Mapper.Map<IPartialView, CodeFileDisplay>(viewMacro);
                case "script":
                    var script = Services.FileService.GetScriptByName(virtualPath);
                    return script == null ? null : Mapper.Map<Script, CodeFileDisplay>(script);
                default:
                    throw new ArgumentException("File Type not supported", "type");
            }
        }

        [HttpDelete]
        public HttpResponseMessage Delete(string type, string virtualPath)
        {
            switch (type)
            {
                case "partialView":
                    if (Services.FileService.DeletePartialView(virtualPath, Security.CurrentUser.Id))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound, "No Partial View found with the specified path");
                    }
                    break;
                case "partialViewMacro":
                    if (Services.FileService.DeletePartialViewMacro(virtualPath, Security.CurrentUser.Id))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound, "No Partial View Macro found with the specified path");
                    }
                    break;
                case "script":
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
                    throw new ArgumentException("File Type not supported", "type");
            }
        }

        public CodeFileDisplay PostSave(string type, CodeFileDisplay display)
        {
            if (display == null)
            {
                throw new ArgumentNullException("No file object has been passed");
            }
            else
            {
                switch (type)
                {
                    case "partialView":
                        var view = Services.FileService.GetPartialView(display.VirtualPath);
                        if (view != null)
                        {
                            view.Content = display.Content;
                            Services.FileService.SavePartialView(view, Security.CurrentUser.Id);
                        }
                        else
                        {
                            throw new ArgumentNullException($"File doesn't exist - {display.VirtualPath}");
                        }
                        break;
                    case "partialViewMacro":
                        var viewMacro = Services.FileService.GetPartialViewMacro(display.VirtualPath);
                        if (viewMacro != null)
                        {
                            viewMacro.Content = display.Content;
                            Services.FileService.SavePartialViewMacro(viewMacro, Security.CurrentUser.Id);
                        }
                        else
                        {
                            throw new ArgumentNullException($"File doesn't exist - {display.VirtualPath}");
                        }
                        break;
                    case "script":
                        var script = Services.FileService.GetScriptByName(display.VirtualPath);
                        if (script != null)
                        {
                            script.Content = display.Content;
                            Services.FileService.SaveScript(script, Security.CurrentUser.Id);
                        }
                        else
                        {
                            throw new ArgumentNullException($"File doesn't exist - {display.VirtualPath}");
                        }
                        break;
                    default:
                        throw new ArgumentException("File Type not supported", "type");
                }
                return display;
            }
        }
    }
}
