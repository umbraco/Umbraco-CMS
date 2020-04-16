using System.Collections.Generic;

namespace Umbraco.Web.Models.PropertyEditorsConfigs
{
    public class RichTextSettings
    {
        public string Mode { get; set; }
        public int MaxImageSize { get; set; }
        public IList<string> Toolbar { get; set; }
        public IList<string> Stylesheets { get; set; }
    }
}
