using System.Runtime.Serialization;

namespace Umbraco.Core.Models.Header
{
    /// <summary>
    /// Default app that will execute an action when clicked on
    /// </summary>
    public class DefaultHeaderApp : HeaderApp
    {
        public DefaultHeaderApp()
        {
            View = "views/header/apps/default/default.html";
        }

        [DataMember(Name = "action")]
        public string Action { get; set; }

        [DataMember(Name = "hotkey")]
        public string Hotkey { get; set; }
    }
}
