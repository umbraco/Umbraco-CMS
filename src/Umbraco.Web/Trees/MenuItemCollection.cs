using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Core;
using umbraco.interfaces;

namespace Umbraco.Web.Trees
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

        /// <summary>
        /// Adds a menu item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public MenuItem AddMenuItem<T>()
            where T : IAction
        {
            return AddMenuItem<T>(null);
        }

        public MenuItem AddMenuItem<T>(bool hasSeparator)
            where T : IAction
        {
            var item = AddMenuItem<T>();
            item.SeperatorBefore = hasSeparator;
            return item;
        }

        /// <summary>
        /// Adds a menu item with a key value pair which is merged to the AdditionalData bag
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public MenuItem AddMenuItem<T>(string key, string value)
            where T : IAction
        {
            return AddMenuItem<T>(new Dictionary<string, object> { { key, value } });
        }

        /// <summary>
        /// Adds a menu item with a dictionary which is merged to the AdditionalData bag
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="additionalData"></param>
        public MenuItem AddMenuItem<T>(IDictionary<string, object> additionalData)
            where T : IAction
        {
            var item = ActionsResolver.Current.GetAction<T>();
            if (item != null)
            {
                var menuItem = new MenuItem(item);
                
                if (additionalData != null)
                {
                    foreach (var i in additionalData)
                    {
                        menuItem.AdditionalData[i.Key] = i.Value;
                    }
                }

                _menuItems.Add(menuItem);

                //TODO: Once we implement 'real' menu items, not just IActions we can implement this since
                // people may need to pass specific data to their menu items

                ////validate the data in the meta data bag
                //item.ValidateRequiredData(AdditionalData);

                return menuItem;
            }
            return null;
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