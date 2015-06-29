using System;
using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Threading;
using System.Xml;
using System.Linq;
using Umbraco.Core;
using umbraco.cms.businesslogic.language;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using umbraco.DataLayer;
using umbraco.BusinessLogic;
using System.Runtime.CompilerServices;
using Language = umbraco.cms.businesslogic.language.Language;

namespace umbraco.cms.businesslogic
{
    [Obsolete("Obsolete, Umbraco.Core.Services.ILocalizationService")]
    public class Dictionary
    {
        private static readonly Guid TopLevelParent = new Guid(Constants.Conventions.Localization.DictionaryItemRootId);

        [Obsolete("Obsolete, For querying the database use the new UmbracoDatabase object ApplicationContext.Current.DatabaseContext.Database")]
        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }
        
        /// <summary>
        /// Retrieve a list of toplevel DictionaryItems
        /// </summary>
        public static DictionaryItem[] getTopMostItems
        {
            get
            {
                return ApplicationContext.Current.Services.LocalizationService.GetRootDictionaryItems()
                    .Select(x => new DictionaryItem(x))
                    .ToArray();
            }
        }

        /// <summary>
        /// A DictionaryItem is basically a key/value pair (key/language key/value) which holds the data
        /// associated to a key in various language translations
        /// </summary>
        public class DictionaryItem
        {
            public DictionaryItem()
            {
                
            }

            internal DictionaryItem(IDictionaryItem item)
            {
                _dictionaryItem = item;
            }

            private readonly IDictionaryItem _dictionaryItem;
            private DictionaryItem _parent;
           
            public DictionaryItem(string key)
            {
                _dictionaryItem = ApplicationContext.Current.Services.LocalizationService.GetDictionaryItemByKey(key);

                if (_dictionaryItem == null)
                {
                    throw new ArgumentException("No key " + key + " exists in dictionary");
                }
            }

            public DictionaryItem(Guid id)
            {
                _dictionaryItem = ApplicationContext.Current.Services.LocalizationService.GetDictionaryItemById(id);

                if (_dictionaryItem == null)
                {
                    throw new ArgumentException("No unique id " + id + " exists in dictionary");
                }
            }

            public DictionaryItem(int id)
            {
                _dictionaryItem = ApplicationContext.Current.Services.LocalizationService.GetDictionaryItemById(id);

                if (_dictionaryItem == null)
                {
                    throw new ArgumentException("No id " + id + " exists in dictionary");
                }
            }

            [Obsolete("This is no longer used and will be removed from the codebase in future versions")]
            public bool IsTopMostItem()
            { 
                return _dictionaryItem.ParentId == new Guid(Constants.Conventions.Localization.DictionaryItemRootId);
            }

            /// <summary>
            /// Returns the parent.
            /// </summary>
            public DictionaryItem Parent
            {
                get
                {
                    //EnsureCache();
                    if (_parent == null)
                    {
                        var p = ApplicationContext.Current.Services.LocalizationService.GetDictionaryItemById(_dictionaryItem.ParentId);

                        if (p == null)
                        {
                            throw new ArgumentException("Top most dictionary items doesn't have a parent");
                        }
                        else
                        {
                            _parent = new DictionaryItem(p);
                        }
                    }

                    return _parent;
                }
            }

            /// <summary>
            /// The primary key in the database
            /// </summary>
            public int id
            {
                get { return _dictionaryItem.Id; }
            }

            public DictionaryItem[] Children
            {
                get
                {
                    return ApplicationContext.Current.Services.LocalizationService.GetDictionaryItemChildren(_dictionaryItem.Key)
                        .WhereNotNull()
                        .Select(x => new DictionaryItem(x))
                        .ToArray();
                }
            }

            public static bool hasKey(string key)
            {
                return ApplicationContext.Current.Services.LocalizationService.DictionaryItemExists(key);
            }

            public bool hasChildren
            {
                get { return Children.Any(); }
            }

            /// <summary>
            /// Returns or sets the key.
            /// </summary>
            public string key
            {
                get { return _dictionaryItem.ItemKey; }
                set
                {
                    if (hasKey(value) == false)
                    {                        
                        _dictionaryItem.ItemKey = value;
                    }
                    else
                        throw new ArgumentException("New value of key already exists (is key)");
                }
            }

            public string Value(int languageId)
            {
                if (languageId == 0)
                    return Value();

                var translation = _dictionaryItem.Translations.FirstOrDefault(x => x.Language.Id == languageId);
                return translation == null ? string.Empty : translation.Value;
            }

            public void setValue(int languageId, string value)
            {
                ApplicationContext.Current.Services.LocalizationService.AddOrUpdateDictionaryValue(
                    _dictionaryItem,
                    ApplicationContext.Current.Services.LocalizationService.GetLanguageById(languageId),
                    value);

                Save();
            }

            /// <summary>
            /// Returns the default value based on the default language for this item
            /// </summary>
            /// <returns></returns>
            public string Value()
            {
                var defaultTranslation = _dictionaryItem.Translations.FirstOrDefault(x => x.Language.Id == 1);
                return defaultTranslation == null ? string.Empty : defaultTranslation.Value;
            }

            [EditorBrowsable(EditorBrowsableState.Never)]
            [Obsolete("This is not used and should never be used, it will be removed from the codebase in future versions")]
            public void setValue(string value)
            {
                if (Item.hasText(_dictionaryItem.Key, 0))
                    Item.setText(0, _dictionaryItem.Key, value);
                else
                    Item.addText(0, _dictionaryItem.Key, value);

                Save();
            }

            public static int addKey(string key, string defaultValue, string parentKey)
            {
                //EnsureCache();

                if (hasKey(parentKey))
                {
                    int retval = CreateKey(key, new DictionaryItem(parentKey)._dictionaryItem.Key, defaultValue);
                    return retval;
                }
                else
                    throw new ArgumentException("Parentkey doesnt exist");
            }

            public static int addKey(string key, string defaultValue)
            {
                int retval = CreateKey(key, TopLevelParent, defaultValue);
                return retval;
            }

            public void delete()
            {
                OnDeleting(EventArgs.Empty);

                ApplicationContext.Current.Services.LocalizationService.Delete(_dictionaryItem);

                OnDeleted(EventArgs.Empty);
            }

            /// <summary>
            /// ensures events fire after setting proeprties
            /// </summary>
            public void Save()
            {
                OnSaving(EventArgs.Empty);

                ApplicationContext.Current.Services.LocalizationService.Save(_dictionaryItem);
            }


            public XmlNode ToXml(XmlDocument xd)
            {
                var serializer = new EntityXmlSerializer();
                var xml = serializer.Serialize(_dictionaryItem);
                var xmlNode = xml.GetXmlNode(xd);
                if (this.hasChildren)
                {
                    foreach (var di in this.Children)
                    {
                        xmlNode.AppendChild(di.ToXml(xd));
                    }
                }
                return xmlNode;
            }

            public static DictionaryItem Import(XmlNode xmlData)
            {
                return Import(xmlData, null);
            }

            public static DictionaryItem Import(XmlNode xmlData, DictionaryItem parent)
            {
                string key = xmlData.Attributes["Key"].Value;

                XmlNodeList values = xmlData.SelectNodes("./Value");
                XmlNodeList childItems = xmlData.SelectNodes("./DictionaryItem");
                DictionaryItem newItem;
                bool retVal = false;

                if (!hasKey(key))
                {
                    if (parent != null)
                        addKey(key, " ", parent.key);
                    else
                        addKey(key, " ");

                    if (values.Count > 0)
                    {
                        //Set language values on the dictionary item
                        newItem = new DictionaryItem(key);
                        foreach (XmlNode xn in values)
                        {
                            string cA = xn.Attributes["LanguageCultureAlias"].Value;
                            string keyValue = xmlHelper.GetNodeValue(xn);

                            Language valueLang = Language.GetByCultureCode(cA);

                            if (valueLang != null)
                            {
                                newItem.setValue(valueLang.id, keyValue);
                            }
                        }
                    }

                    if (parent == null)
                        retVal = true;
                }

                newItem = new DictionaryItem(key);

                foreach (XmlNode childItem in childItems)
                {
                    Import(childItem, newItem);
                }

                if (retVal)
                    return newItem;
                else
                    return null;
            }

            private static int CreateKey(string key, Guid parentId, string defaultValue)
            {
                if (!hasKey(key))
                {
                    var item = ApplicationContext.Current.Services.LocalizationService.CreateDictionaryItemWithIdentity(
                        key, parentId, defaultValue);

                    return item.Id;
                }
                else
                {
                    throw new ArgumentException("Key being added already exists!");
                }
            }

            #region Events
            public delegate void SaveEventHandler(DictionaryItem sender, EventArgs e);
            public delegate void NewEventHandler(DictionaryItem sender, EventArgs e);
            public delegate void DeleteEventHandler(DictionaryItem sender, EventArgs e);

            public static event SaveEventHandler Saving;
            protected virtual void OnSaving(EventArgs e)
            {
                if (Saving != null)
                    Saving(this, e);
            }

            public static event NewEventHandler New;
            protected virtual void OnNew(EventArgs e)
            {
                if (New != null)
                    New(this, e);
            }

            public static event DeleteEventHandler Deleting;
            protected virtual void OnDeleting(EventArgs e)
            {
                if (Deleting != null)
                    Deleting(this, e);
            }

            public static event DeleteEventHandler Deleted;
            protected virtual void OnDeleted(EventArgs e)
            {
                if (Deleted != null)
                    Deleted(this, e);
            }
            #endregion
        }

        // zb023 - utility method
        public static string ReplaceKey(string text)
        {
            if (text.StartsWith("#") == false)
                return text;

            var lang = Language.GetByCultureCode(Thread.CurrentThread.CurrentCulture.Name);

            if (lang == null)
                return "[" + text + "]";

            if (DictionaryItem.hasKey(text.Substring(1, text.Length - 1)) == false)
                return "[" + text + "]";

            var di = new DictionaryItem(text.Substring(1, text.Length - 1));
            return di.Value(lang.id);
        }

    }
}