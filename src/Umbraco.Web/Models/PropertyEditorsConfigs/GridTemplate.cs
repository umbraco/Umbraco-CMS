using System.Collections.Generic;

namespace Umbraco.Web.Models.PropertyEditorsConfigs
{
    public class GridTemplate
    {
        public string Name { get; set; }
        public List<GridSection> Sections{ get; set; }
    }
}
