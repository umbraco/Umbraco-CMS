﻿using System.Collections.Generic;
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

        public MenuItemList( IEnumerable<MenuItem> items)
            : base(items)
        {
        }

        /// <summary>
        /// Adds a menu item with a dictionary which is merged to the AdditionalData bag
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hasSeparator"></param>
        /// <param name="textService">The <see cref="ILocalizedTextService"/> used to localize the action name based on its alias</param>
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

        private MenuItem CreateMenuItem<T>(ILocalizedTextService textService, bool hasSeparator = false, bool opensDialog = false)
            where T : IAction
        {
            var item = Current.Actions.GetAction<T>();
            if (item == null) return null;

            var menuItem = new MenuItem(item, textService.Localize($"actions/{item.Alias}"))
            {
                SeparatorBefore = hasSeparator,
                OpensDialog = opensDialog
            };

            return menuItem;
        }

    }
}
