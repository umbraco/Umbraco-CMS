using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Umbraco.Extensions;

public static class ViewContextExtensions
{
    /// <summary>
    ///     Creates a new ViewContext from an existing one but specifies a new Model for the ViewData
    /// </summary>
    /// <param name="vc"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    public static ViewContext CopyWithModel(this ViewContext vc, object model) =>
        new ViewContext
        {
            View = vc.View,
            Writer = vc.Writer,
            ActionDescriptor = vc.ActionDescriptor,
            FormContext = vc.FormContext,
            HttpContext = vc.HttpContext,
            RouteData = vc.RouteData,
            TempData = vc.TempData,
            ViewData = new ViewDataDictionary(vc.ViewData) { Model = model },
            ClientValidationEnabled = vc.ClientValidationEnabled,
            ExecutingFilePath = vc.ExecutingFilePath,
            ValidationMessageElement = vc.ValidationMessageElement,
            Html5DateRenderingMode = vc.Html5DateRenderingMode,
            ValidationSummaryMessageElement = vc.ValidationSummaryMessageElement,
        };

    public static ViewContext Clone(this ViewContext vc) =>
        new ViewContext
        {
            View = vc.View,
            Writer = vc.Writer,
            ActionDescriptor = vc.ActionDescriptor,
            FormContext = vc.FormContext,
            HttpContext = vc.HttpContext,
            RouteData = vc.RouteData,
            TempData = vc.TempData,
            ViewData = new ViewDataDictionary(vc.ViewData),
            ClientValidationEnabled = vc.ClientValidationEnabled,
            ExecutingFilePath = vc.ExecutingFilePath,
            ValidationMessageElement = vc.ValidationMessageElement,
            Html5DateRenderingMode = vc.Html5DateRenderingMode,
            ValidationSummaryMessageElement = vc.ValidationSummaryMessageElement,
        };

    // public static ViewContext CloneWithWriter(this ViewContext vc, TextWriter writer)
    // {
    //    return new ViewContext
    //    {
    //        Controller = vc.Controller,
    //        HttpContext = vc.HttpContext,
    //        RequestContext = vc.RequestContext,
    //        RouteData = vc.RouteData,
    //        TempData = vc.TempData,
    //        View = vc.View,
    //        ViewData = vc.ViewData,
    //        FormContext = vc.FormContext,
    //        ClientValidationEnabled = vc.ClientValidationEnabled,
    //        UnobtrusiveJavaScriptEnabled = vc.UnobtrusiveJavaScriptEnabled,
    //        Writer = writer
    //    };
    // }
}
