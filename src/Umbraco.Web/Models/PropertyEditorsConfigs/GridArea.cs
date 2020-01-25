using System.Collections.Generic;

namespace Umbraco.Web.Models.PropertyEditorsConfigs
{
    public class GridArea
    {
        public int Grid { get; set; }
        public bool AllowAll { get; set; }
        public List<string> Allowed { get; set; }
    }
}
