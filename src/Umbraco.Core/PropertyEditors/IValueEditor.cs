namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// An interface that is shared between parameter and property value editors to access their views
    /// </summary>
    public interface IValueEditor
    {
        string View { get; }
    }
}