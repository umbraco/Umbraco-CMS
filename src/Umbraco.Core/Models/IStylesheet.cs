namespace Umbraco.Cms.Core.Models;

public interface IStylesheet : IFile
{
    /// <summary>
    ///     Returns a list of umbraco back office enabled stylesheet properties
    /// </summary>
    /// <remarks>
    ///     An umbraco back office enabled stylesheet property has a special prefix, for example:
    ///     /** umb_name: MyPropertyName */ p { font-size: 1em; }
    /// </remarks>
    IEnumerable<IStylesheetProperty>? Properties { get; }

    /// <summary>
    ///     Adds an Umbraco stylesheet property for use in the back office
    /// </summary>
    /// <param name="property"></param>
    void AddProperty(IStylesheetProperty property);

    /// <summary>
    ///     Removes an Umbraco stylesheet property
    /// </summary>
    /// <param name="name"></param>
    void RemoveProperty(string name);
}
