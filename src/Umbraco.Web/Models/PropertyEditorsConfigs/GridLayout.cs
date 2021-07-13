using System.Collections.Generic;

namespace Umbraco.Web.Models.PropertyEditorsConfigs
{
    public class GridLayout
    {
        public string Name { get; set; }
        public IList<GridArea> Areas { get; set; }
    }
}
