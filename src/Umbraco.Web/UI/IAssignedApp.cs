namespace Umbraco.Web.UI
{
    /// <summary>
    /// This is used for anything that is assigned to an app
    /// </summary>
    /// <remarks>
    /// Currently things that need to be assigned to an app in order for user security to work are:
    /// dialogs, ITasks, editors
    /// </remarks>
    public interface IAssignedApp
    {
        /// <summary>
        /// Returns the app alias that this element belongs to
        /// </summary>
        string AssignedApp { get; }
    }
}