using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Web.Common.Views;

namespace Umbraco.Cms.Web.Common.Macros;

/// <summary>
///     The base view class that PartialViewMacro views need to inherit from
/// </summary>
public abstract class PartialViewMacroPage : UmbracoViewPage<PartialViewMacroModel>
{
}
