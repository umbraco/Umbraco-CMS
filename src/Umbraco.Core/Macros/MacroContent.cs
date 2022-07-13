namespace Umbraco.Cms.Core.Macros;

// represents the content of a macro
public class MacroContent
{
    // gets an empty macro content
    public static MacroContent Empty { get; } = new();

    // gets or sets the text content
    public string? Text { get; set; }

    // gets or sets the date the content was generated
    public DateTime Date { get; set; } = DateTime.Now;

    // a value indicating whether the content is empty
    public bool IsEmpty => Text is null;
}
