using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Macros
{
    /// <summary>
    /// The base view class that PartialViewMacro views need to inherit from
    /// </summary>
    public abstract class PartialViewMacroPage : UmbracoViewPage<PartialViewMacroModel>
    { }
}
