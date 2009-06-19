using umbraco.presentation.LiveEditing.Menu;
using umbraco.presentation.LiveEditing.Updates;

namespace umbraco.presentation.LiveEditing
{
    /// <summary>
    /// Interfaces that encapsulates all Live Editing information of a specific Umbraco context.
    /// </summary>
    public interface ILiveEditingContext
    {
        /// <summary>
        /// Gets or sets whether Live Editing is enabled.
        /// </summary>
        /// <value><c>true</c> if Live Editing enabled; otherwise, <c>false</c>.</value>
        bool Enabled { get; set;  }

        /// <summary>
        /// Gets the Live Editing menu.
        /// </summary>
        /// <value>The Live Editing menu, or <c>null</c> if Live Editing is disabled.</value>
        ILiveEditingMenu Menu { get; }

        /// <summary>
        /// Gets the field updates made during this context.
        /// </summary>
        /// <value>The updates.</value>
        IUpdateList Updates { get; }
    }
}
