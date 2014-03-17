using System;
using System.Web;
using System.Web.Http.Controllers;
using Umbraco.Core;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// This allows for calling GetById/GetByIds with a GUID... so it will automatically route to GetByKey/GetByKeys
    /// </summary>
    internal class EntityControllerActionSelector : ApiControllerActionSelector 
    {

        public override HttpActionDescriptor SelectAction(HttpControllerContext controllerContext)
        {
            if (controllerContext.Request.RequestUri.GetLeftPart(UriPartial.Path).InvariantEndsWith("GetById"))
            {
                var id = HttpUtility.ParseQueryString(controllerContext.Request.RequestUri.Query).Get("id");

                if (id != null)
                {
                    Guid parsed;
                    if (Guid.TryParse(id, out parsed))
                    {
                        var controllerType = controllerContext.Controller.GetType();
                        var method = controllerType.GetMethod("GetByKey");
                        if (method != null)
                        {
                            return new ReflectedHttpActionDescriptor(controllerContext.ControllerDescriptor, method);    
                        }                        
                    }
                }
            }

            if (controllerContext.Request.RequestUri.GetLeftPart(UriPartial.Path).InvariantEndsWith("GetByIds"))
            {
                var ids = HttpUtility.ParseQueryString(controllerContext.Request.RequestUri.Query).GetValues("ids");

                if (ids != null)
                {
                    var allmatched = true;
                    foreach (var id in ids)
                    {
                        Guid parsed;
                        if (Guid.TryParse(id, out parsed) == false)
                        {
                            allmatched = false;                            
                        }
                    }
                    if (allmatched)
                    {
                        var controllerType = controllerContext.Controller.GetType();
                        var method = controllerType.GetMethod("GetByKeys");
                        if (method != null)
                        {
                            return new ReflectedHttpActionDescriptor(controllerContext.ControllerDescriptor, method);
                        }
                    }
                }
            }

            

            return base.SelectAction(controllerContext);
        }

    }
}