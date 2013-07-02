namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a section defined in the app.config file
    /// </summary>
    public class Section
    {
        public Section(string name, string @alias, string icon, int sortOrder)
        {
            Name = name;
            Alias = alias;
            Icon = icon;
            SortOrder = sortOrder;
        }

        public Section()
        {
            
        }

        public string Name { get; set; }
        public string Alias { get; set; }
        public string Icon { get; set; }
        public int SortOrder { get; set; }
    }
}