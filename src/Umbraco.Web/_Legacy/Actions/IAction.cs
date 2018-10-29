using Umbraco.Core.Composing;

namespace Umbraco.Web._Legacy.Actions
{
    public interface IAction : IDiscoverable
    {
        char Letter { get; }
        bool ShowInNotifier { get; }
        bool CanBePermissionAssigned { get; }
        string Icon { get; }
        string Alias { get; }
        string JsFunctionName { get; }
        /// <summary>
        /// A path to a supporting JavaScript file for the IAction. A script tag will be rendered out with the reference to the JavaScript file.
        /// </summary>
        string JsSource { get; }
        /// <summary>
        /// Whether or not the action opens a dialog when invoked
        /// </summary>
        bool OpensDialog { get; }
    }
}
