using System.Collections.Generic;

namespace Umbraco.Web.Models.PropertyEditorsConfigs
{
    public class GridTemplate
    {
        public string Name { get; set; }
        public IList<GridSection> Sections { get; set; }
    }
}
