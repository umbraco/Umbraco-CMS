using System.Web.UI;

namespace umbraco.interfaces
{
    /// <summary>
    /// Data type editor controls can choose to implement this interface
    /// to customize their Live Editing behavior.
    /// </summary>
    public interface ILiveEditingDataEditor
    {
        /// <summary>
        /// Gets the control used for Live Editing.
        /// </summary>
        Control LiveEditingControl { get; }
    }
}
