using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;

namespace Umbraco.Core
{
    internal static class ControlExtensions
    {
        /// <summary>
        /// Recursively finds a control with the specified identifier.
        /// </summary>
        /// <typeparam name="T">
        /// The type of control to be found.
        /// </typeparam>
        /// <param name="parent">
        /// The parent control from which the search will start.
        /// </param>
        /// <param name="id">
        /// The identifier of the control to be found.
        /// </param>
        /// <returns>
        /// The control with the specified identifier, otherwise <see langword="null"/> if the control 
        /// is not found.
        /// </returns>
        public static T FindControlRecursive<T>(this Control parent, string id) where T : Control
        {
            if ((parent is T) && (parent.ID == id))
            {
                return (T)parent;
            }

            foreach (Control control in parent.Controls)
            {
                var foundControl = FindControlRecursive<T>(control, id);
                if (foundControl != null)
                {
                    return foundControl;
                }
            }
            return default(T);
        }

    }
}
