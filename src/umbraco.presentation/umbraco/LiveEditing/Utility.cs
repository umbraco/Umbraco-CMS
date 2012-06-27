using System;
using System.Collections.Generic;
using System.Web.UI;

namespace umbraco.presentation.LiveEditing
{
    /// <summary>
    /// Utility that simplifies some commmon tasks for Live Editing.
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Finds a control satisfying a matcher recursively.
        /// </summary>
        /// <typeparam name="T">Type of the control to find.</typeparam>
        /// <param name="matcher">The matcher.</param>
        /// <param name="root">Control where the search should start at.</param>
        /// <returns>The control if found; otherwise <c>null</c>.</returns>
        public static T FindControl<T>(Predicate<T> matcher, Control root) where T : Control
        {
            // is this item the one we are looking for?
            if (root is T && matcher((T)root))
                return (T)root;

            // try to find the item in child controls recursively
            foreach (Control child in root.Controls)
            {
                T foundControl = FindControl(matcher, child);
                if (foundControl != null)
                    return foundControl;
            }
            return null;
        }

        /// <summary>
        /// Finds all controls of the given type.
        /// </summary>
        /// <typeparam name="T">Type of the control to find.</typeparam>
        /// <param name="root">Control where the search should start at.</param>
        /// <returns>All satisfying controls.</returns>
        public static List<T> FindControls<T>(Control root) where T : Control
        {
            return FindControls<T>(t => true, root);
        }

        /// <summary>
        /// Finds all controls satisfying a matcher recursively.
        /// </summary>
        /// <typeparam name="T">Type of the control to find.</typeparam>
        /// <param name="matcher">The matcher.</param>
        /// <param name="root">Control where the search should start at.</param>
        /// <returns>All satisfying controls.</returns>
        public static List<T> FindControls<T>(Predicate<T> matcher, Control root) where T : Control
        {
            List<T> controls = new List<T>();

             // add current control if it matches
            if (root is T && matcher((T)root))
                controls.Add((T)root);

             // add matching child controls recursively
            foreach (Control child in root.Controls)
                 controls.AddRange(FindControls(matcher, child));

             return controls;
        }
    }
}
