namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a section defined in the app.config file.
    /// </summary>
    public class Section
    {
        public Section(string name, string @alias, int sortOrder)
        {
            Name = name;
            Alias = alias;
            SortOrder = sortOrder;
        }

        public Section()
        { }

        public string Name { get; set; }
        public string Alias { get; set; }
        public int SortOrder { get; set; }
    }
}
