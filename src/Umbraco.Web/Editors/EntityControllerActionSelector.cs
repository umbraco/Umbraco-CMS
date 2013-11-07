using System;
using System.Web;
using System.Web.Http.Controllers;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// This allows for calling GetById with a GUID... so it will automatically route to GetByKey
    /// </summary>
    internal class EntityControllerActionSelector : ApiControllerActionSelector 
    {

        public override HttpActionDescriptor SelectAction(HttpControllerContext controllerContext)
        {
            if (controllerContext.Request.RequestUri.GetLeftPart(UriPartial.Path).ToUpper().EndsWith("GETBYID"))
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

            

            return base.SelectAction(controllerContext);
        }

    }
}