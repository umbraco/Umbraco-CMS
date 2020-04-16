using System.Collections.Generic;

namespace Umbraco.Web.Models.PropertyEditorsConfigs
{
    public class GridItems
    {
        public IList<GridStyle> Styles { get; set; }
        public IList<GridConfig> Config { get; set; }
        public int Columns { get; set; }
        public IList<GridTemplate> Templates { get; set; }
        public IList<GridLayout> Layouts { get; set; }

    }
}
