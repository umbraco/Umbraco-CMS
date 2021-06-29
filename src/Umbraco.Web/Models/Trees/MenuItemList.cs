using System;
using System.Collections.Generic;
using System.Threading;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Web.Actions;
using Umbraco.Web.Composing;


namespace Umbraco.Web.Models.Trees
{
    /// <summary>
    /// A custom menu list
    /// </summary>
    /// <remarks>
    /// NOTE: We need a sub collection to the MenuItemCollection object due to how json serialization works.
    /// </remarks>
    public class MenuItemList : List<MenuItem>
    {
        public MenuItemList()
        {
        }

        public MenuItemList(IEnumerable<MenuItem> items)
            : base(items)
        {
        }

        /// <summary>
        /// Adds a menu item based on a <see cref="IAction"/>
        /// </summary>
        /// <param name="action"></param>
        /// <param name="name">The text to display for the menu item, will default to the IAction alias if not specified</param>
        internal MenuItem Add(IAction action, string name)
        {
            var item = new MenuItem(action, name);

            Add(item);
            return item;
        }

        /// <summary>
        /// Adds a menu item with a dictionary which is merged to the AdditionalData bag
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hasSeparator"></param>
        /// <param name="name">The text to display for the menu item, will default to the IAction alias if not specified</param>
        /// <param name="opensDialog">Whether or not this action opens a dialog</param>
        public MenuItem Add<T>(string name, bool hasSeparator = false, bool opensDialog = false)
            where T : IAction
        {
            var item = CreateMenuItem<T>(name, hasSeparator, opensDialog);
            if (item != null)
            {
                Add(item);
                return item;
            }
            return null;
        }

        /// <summary>
        /// Adds a menu item with a dictionary which is merged to the AdditionalData bag
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hasSeparator"></param>
        /// <param name="textService">The <see cref="ILocalizedTextService"/> used to localize the action name based on it's alias</param>
        /// <param name="opensDialog">Whether or not this action opens a dialog</param>
        public MenuItem Add<T>(ILocalizedTextService textService, bool hasSeparator = false, bool opensDialog = false)
            where T : IAction
        {
            var item = CreateMenuItem<T>(textService, hasSeparator, opensDialog);
            if (item != null)
            {
                Add(item);
                return item;
            }
            return null;
        }

        internal MenuItem CreateMenuItem<T>(string name, bool hasSeparator = false, bool opensDialog = false)
            where T : IAction
        {
            var item = Current.Actions.GetAction<T>();
            if (item == null) return null;
            var menuItem = new MenuItem(item, name)
            {
                SeparatorBefore = hasSeparator,
                OpensDialog = opensDialog
            };

            return menuItem;
        }

        internal MenuItem CreateMenuItem<T>(ILocalizedTextService textService, bool hasSeparator = false, bool opensDialog = false)
            where T : IAction
        {
            var item = Current.Actions.GetAction<T>();
            if (item == null) return null;


            var menuItem = new MenuItem(item, textService.Localize("actions",item.Alias))
            {
                SeparatorBefore = hasSeparator,
                OpensDialog = opensDialog,
                TextDescription = textService.Localize("visuallyHiddenTexts", item.Alias+"Description", Thread.CurrentThread.CurrentUICulture),
            };

            return menuItem;
        }

    }
}
