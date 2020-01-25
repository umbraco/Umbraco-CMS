using System.Collections.Generic;

namespace Umbraco.Web.Models.PropertyEditorsConfigs
{
    public class GridSection
    {
        public int grid { set; get; }
        public bool AllowAll { get; set; }
        public List<string> Allowed { get; set; }
    }
}
