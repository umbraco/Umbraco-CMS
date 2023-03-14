using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Umbraco.Cms.Web.Common.Controllers;

/// <summary>
///     A request feature to allowing proxying viewdata from one controller to another
/// </summary>
public sealed class ProxyViewDataFeature
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ProxyViewDataFeature" /> class.
    /// </summary>
    public ProxyViewDataFeature(ViewDataDictionary viewData, ITempDataDictionary tempData)
    {
        ViewData = viewData;
        TempData = tempData;
    }

    /// <summary>
    ///     Gets the <see cref="ViewDataDictionary" />
    /// </summary>
    public ViewDataDictionary ViewData { get; }

    /// <summary>
    ///     Gets the <see cref="ITempDataDictionary" />
    /// </summary>
    public ITempDataDictionary TempData { get; }
}
