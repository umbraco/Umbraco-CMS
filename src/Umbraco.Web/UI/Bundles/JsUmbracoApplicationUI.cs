using System.Web.UI;
using ClientDependency.Core;

namespace Umbraco.Web.UI.Bundles
{
    /// <summary>
    /// The UI Umbraco libs
    /// </summary>
    [ClientDependency(ClientDependencyType.Javascript, "js/guiFunctions.js", "UmbracoRoot", Priority = 1, Group = 5)]
    [ClientDependency(ClientDependencyType.Javascript, "js/UmbracoSpeechBubbleBackend.js", "UmbracoRoot", Priority = 3, Group = 5)]
    public class JsUmbracoApplicationUI : Control
    {
    }
}
