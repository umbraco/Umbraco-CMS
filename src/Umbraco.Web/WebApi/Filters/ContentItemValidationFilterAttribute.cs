using System;
using System.Collections.Generic;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.UI;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Models.Mapping;

namespace Umbraco.Web.WebApi.Filters
{
    ///// <summary>
    ///// Validates the content item
    ///// </summary>
    ///// <remarks>
    ///// There's various validation happening here both value validation and structure validation
    ///// to ensure that malicious folks are not trying to post invalid values or to invalid properties.
    ///// </remarks>
    //internal class ContentItemValidationFilterAttribute : ActionFilterAttribute 
    //{
    //    private readonly Type _helperType;
    //    private readonly dynamic _helper;

    //    public ContentItemValidationFilterAttribute(Type helperType)
    //    {
    //        _helperType = helperType;
    //        _helper = Activator.CreateInstance(helperType);
    //    }

    //    /// <summary>
    //    /// Returns true so that other filters can execute along with this one
    //    /// </summary>
    //    public override bool AllowMultiple
    //    {
    //        get { return true; }
    //    }

    //    /// <summary>
    //    /// Performs the validation
    //    /// </summary>
    //    /// <param name="actionContext"></param>
    //    public override void OnActionExecuting(HttpActionContext actionContext)
    //    {
    //        _helper.ValidateItem(actionContext);
    //    }

        

    //}
}