using System.Collections.Generic;

namespace Umbraco.Web.Models.PropertyEditorsConfigs
{
    public class GridSection
    {
        public int Grid { set; get; }
        public bool AllowAll { get; set; }
        public List<string> Allowed { get; set; }
    }
}
