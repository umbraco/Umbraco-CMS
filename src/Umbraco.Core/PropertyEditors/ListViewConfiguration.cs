namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the listview value editor.
/// </summary>
public class ListViewConfiguration
{
    public ListViewConfiguration() =>
        // initialize defaults
        IncludeProperties = new[]
        {
            new Property { Alias = "sortOrder", IsSystem = true },
            new Property { Alias = "updateDate", IsSystem = true },
            new Property { Alias = "owner", IsSystem = true },
        };

    [ConfigurationField("includeProperties")]
    public Property[] IncludeProperties { get; set; }

    public class Property
    {
        public string? Alias { get; set; }

        public bool IsSystem { get; set; }
    }
}
