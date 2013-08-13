using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Core;
using umbraco.BusinessLogic.Actions;
using umbraco.interfaces;

namespace Umbraco.Web.Trees.Menu
{

    [CollectionDataContract(Name = "menuItems", Namespace = "")]
    public class MenuItemCollection : IEnumerable<MenuItem>
    {
        public MenuItemCollection()
        {
            
        }

        public MenuItemCollection(IEnumerable<MenuItem> items)
        {
            _menuItems = new List<MenuItem>(items);
        }

        private readonly List<MenuItem> _menuItems = new List<MenuItem>();

        /// <summary>
        /// Adds a menu item
        /// </summary>
        internal MenuItem AddMenuItem(IAction action)
        {
            var item = new MenuItem(action);

            DetectLegacyActionMenu(action.GetType(), item);

            _menuItems.Add(item);
            return item;
        }

        /// <summary>
        /// Adds a menu item
        /// </summary>
        public void AddMenuItem(MenuItem item)
        {
            _menuItems.Add(item);
        }

        //TODO: Implement more overloads for MenuItem with dictionary vals

        public TMenuItem AddMenuItem<TMenuItem, TAction>(bool hasSeparator = false, IDictionary<string, object> additionalData = null)
            where TAction : IAction
            where TMenuItem : MenuItem, new()
        {
            var item = CreateMenuItem<TAction>(hasSeparator, additionalData);
            if (item == null) return null;

            var customMenuItem = new TMenuItem
                {
                    Name = item.Alias,
                    Alias = item.Alias,
                    SeperatorBefore = hasSeparator,
                    Icon = item.Icon,
                    Action = item.Action
                };

            _menuItems.Add(customMenuItem);

            return customMenuItem;
        }

        /// <summary>
        /// Adds a menu item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public MenuItem AddMenuItem<T>()
            where T : IAction
        {
            return AddMenuItem<T>(false, null);
        }

        /// <summary>
        /// Adds a menu item with a key value pair which is merged to the AdditionalData bag
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="hasSeparator"></param>
        public MenuItem AddMenuItem<T>(string key, string value, bool hasSeparator = false)
            where T : IAction
        {
            return AddMenuItem<T>(hasSeparator, new Dictionary<string, object> { { key, value } });
        }

        internal MenuItem CreateMenuItem<T>(bool hasSeparator = false, IDictionary<string, object> additionalData = null)
            where T : IAction
        {
            var item = ActionsResolver.Current.GetAction<T>();
            if (item != null)
            {
                var menuItem = new MenuItem(item)
                    {
                        SeperatorBefore = hasSeparator
                    };

                if (additionalData != null)
                {
                    foreach (var i in additionalData)
                    {
                        menuItem.AdditionalData[i.Key] = i.Value;
                    }
                }

                DetectLegacyActionMenu(typeof (T), menuItem);

                //TODO: Once we implement 'real' menu items, not just IActions we can implement this since
                // people may need to pass specific data to their menu items

                ////validate the data in the meta data bag
                //item.ValidateRequiredData(AdditionalData);

                return menuItem;
            }
            return null;
        }

        /// <summary>
        /// Adds a menu item with a dictionary which is merged to the AdditionalData bag
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hasSeparator"></param>
        /// <param name="additionalData"></param>
        public MenuItem AddMenuItem<T>(bool hasSeparator = false, IDictionary<string, object> additionalData = null)
            where T : IAction
        {
            var item = CreateMenuItem<T>(hasSeparator, additionalData);
            if (item != null) 
            {
                _menuItems.Add(item);
                return item;
            }
            return null;
        }

        /// <summary>
        /// Checks if the IAction type passed in is attributed with LegacyActionMenuItemAttribute and if so 
        /// ensures that the correct action metadata is added.
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="menuItem"></param>
        private void DetectLegacyActionMenu(Type actionType, MenuItem menuItem)
        {
            //This checks for legacy IActions that have the LegacyActionMenuItemAttribute which is a legacy hack
            // to make old IAction actions work in v7 by mapping to the JS used by the new menu items
            var attribute = actionType.GetCustomAttribute<LegacyActionMenuItemAttribute>(false);
            if (attribute != null)
            {
                //add the current type to the metadata
                if (attribute.MethodName.IsNullOrWhiteSpace())
                {
                    //if no method name is supplied we will assume that the menu action is the type name of the current menu class
                    menuItem.AdditionalData.Add("jsAction", string.Format("{0}.{1}", attribute.ServiceName, this.GetType().Name));
                }
                else
                {
                    menuItem.AdditionalData.Add("jsAction", string.Format("{0}.{1}", attribute.ServiceName, attribute.MethodName));
                }
            }
        }

        public IEnumerator<MenuItem> GetEnumerator()
        {
            return _menuItems.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}