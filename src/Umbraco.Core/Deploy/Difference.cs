namespace Umbraco.Cms.Core.Deploy;

/// <summary>
/// Represents a difference between two artifacts.
/// </summary>
public class Difference
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Difference" /> class.
    /// </summary>
    /// <param name="title">The title.</param>
    /// <param name="text">The text.</param>
    /// <param name="category">The category.</param>
    public Difference(string title, string? text = null, string? category = null)
    {
        Title = title;
        Text = text;
        Category = category;
    }

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    /// <value>
    /// The title.
    /// </value>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the text.
    /// </summary>
    /// <value>
    /// The text.
    /// </value>
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the category.
    /// </summary>
    /// <value>
    /// The category.
    /// </value>
    public string? Category { get; set; }

    /// <summary>
    /// Converts the difference to a <see cref="string" />.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents the difference.
    /// </returns>
    public override string ToString()
    {
        var s = Title;
        if (!string.IsNullOrWhiteSpace(Category))
        {
            s += string.Format("[{0}]", Category);
        }

        if (!string.IsNullOrWhiteSpace(Text))
        {
            if (s.Length > 0)
            {
                s += ":";
            }

            s += Text;
        }

        return s;
    }
}
