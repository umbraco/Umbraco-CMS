using System.Collections.Generic;

namespace Umbraco.Web.Models.PropertyEditorsConfigs
{
    public class GridBasic
    {
        public string Label { get; set; }
        public string Description { get; set; }
        public string Key { get; set; }
        public string View { get; set; }
        public string Modifier { get; set; }
        public object ApplyTo { get; set; }
        public List<string> Prevalues{ get; set; }
    }
}
