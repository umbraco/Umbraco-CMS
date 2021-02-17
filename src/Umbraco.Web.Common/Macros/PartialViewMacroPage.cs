using Umbraco.Web.Common.Views;
using Umbraco.Web.Models;

namespace Umbraco.Web.Common.Macros
{
    /// <summary>
    /// The base view class that PartialViewMacro views need to inherit from
    /// </summary>
    public abstract class PartialViewMacroPage : UmbracoViewPage<PartialViewMacroModel>
    { }
}
