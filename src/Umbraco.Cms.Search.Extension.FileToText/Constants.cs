namespace Umbraco.Cms.Search.Extension.FileToText;

/// <summary>
/// Constants used by file-to-text indexing.
/// </summary>
public static class Constants
{
    /// <summary>
    /// The postfix appended to a property alias to form the name of the index field holding the property's file text
    /// (for example, <c>myFile_FileText</c>).
    /// </summary>
    public const string TextFieldPostfix = "_FileText";
}
