
using System;

namespace umbraco.interfaces
{
    /// <summary>
    /// IMenuElement is an interface for items in the umbraco backoffice panel menu
    /// </summary>
    [Obsolete("IMenuElement is obsolete and is no longer used, it will be removed from the codebase in future versions")]
    public interface IMenuElement
    {
        /// <summary>
        /// Gets the name of the element.
        /// </summary>
        /// <value>The name of the element.</value>
        string ElementName { get;}
        /// <summary>
        /// Gets the element id pre fix.
        /// </summary>
        /// <value>The element id pre fix.</value>
        string ElementIdPreFix { get;}
        /// <summary>
        /// Gets the element class.
        /// </summary>
        /// <value>The element class.</value>
        string ElementClass { get;}
        /// <summary>
        /// Gets the width of the extra menu.
        /// </summary>
        /// <value>The width of the extra menu.</value>
        int ExtraMenuWidth { get;}
    }
}
