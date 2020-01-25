using System.Collections.Generic;

namespace Umbraco.Web.Models.PropertyEditorsConfigs
{
    public class RichTextSettings
    {
        public string Mode { get; set; }
        public int MaxImageSize { get; set; }
        public List<string> Toolbar { get; set; }
        public List<string> stylesheets { get; set; }
    }
}
