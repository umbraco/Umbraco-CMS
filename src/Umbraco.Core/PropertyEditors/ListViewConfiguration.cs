namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the listview value editor.
/// </summary>
public class ListViewConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ListViewConfiguration"/> class with default properties.
    /// </summary>
    public ListViewConfiguration() =>
        // initialize defaults
        IncludeProperties = new[]
        {
            new Property { Alias = "sortOrder", IsSystem = true },
            new Property { Alias = "updateDate", IsSystem = true },
            new Property { Alias = "owner", IsSystem = true },
        };

    /// <summary>
    /// Gets or sets the properties to include in the list view.
    /// </summary>
    [ConfigurationField("includeProperties")]
    public Property[] IncludeProperties { get; set; }

    /// <summary>
    /// Represents a property column configuration in the list view.
    /// </summary>
    public class Property
    {
        /// <summary>
        /// Gets or sets the alias of the property.
        /// </summary>
        public string? Alias { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a system property.
        /// </summary>
        public bool IsSystem { get; set; }
    }
}
