namespace Umbraco.Cms.Core.Deploy;

public class Difference
{
    public Difference(string title, string? text = null, string? category = null)
    {
        Title = title;
        Text = text;
        Category = category;
    }

    public string Title { get; set; }

    public string? Text { get; set; }

    public string? Category { get; set; }

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
