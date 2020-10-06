using System.Runtime.Serialization;

namespace Umbraco.Core.Models.Header
{
    public class DefaultHeaderApp : HeaderApp
    {
        public DefaultHeaderApp()
        {
            View = "views/header/apps/default/default.html";
        }

        [DataMember(Name = "action")]
        public string Action { get; set; }
    }
}
