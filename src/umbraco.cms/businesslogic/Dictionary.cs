using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Data;
using System.Threading;
using System.Xml;
using System.Linq;
using Umbraco.Core;
using umbraco.cms.businesslogic.language;
using umbraco.DataLayer;
using umbraco.BusinessLogic;
using System.Runtime.CompilerServices;

namespace umbraco.cms.businesslogic
{
    /// <summary>
    /// The Dictionary is used for storing and retrieving language translated textpieces in Umbraco. It uses
    /// umbraco.cms.businesslogic.language.Item class as storage and can be used from the public website of umbraco
    /// all text are cached in memory.
    /// </summary>
    public class Dictionary
    {
        private static volatile bool _cacheIsEnsured = false;
        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();
        private static readonly ConcurrentDictionary<string, DictionaryItem> DictionaryItems = new ConcurrentDictionary<string, DictionaryItem>();
        private static readonly Guid TopLevelParent = new Guid("41c7638d-f529-4bff-853e-59a0c2fb1bde");

        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        /// <summary>
        /// Reads all items from the database and stores in local cache
        /// </summary>
        private static void EnsureCache()
        {
            using (var lck = new UpgradeableReadLock(Locker))
            {
                if (_cacheIsEnsured) return;
                
                lck.UpgradeToWriteLock();

                using (var dr = SqlHelper.ExecuteReader("Select pk, id, [key], parent from cmsDictionary"))
                {
                    while (dr.Read())
                    {
                        //create new dictionaryitem object and put in cache
                        var item = new DictionaryItem(dr.GetInt("pk"),
                                                      dr.GetString("key"),
                                                      dr.GetGuid("id"),
                                                      dr.GetGuid("parent"));

                        DictionaryItems.TryAdd(item.key, item);
                    }
                }

                _cacheIsEnsured = true;
            }            
        }

        /// <summary>
        /// Used by the cache refreshers to clear the cache on distributed servers
        /// </summary>
        internal static void ClearCache()
        {
            using (new WriteLock(Locker))
            {
                DictionaryItems.Clear();
                //ensure the flag is reset so that EnsureCache will re-cache everything
                _cacheIsEnsured = false;
            }
        }

        /// <summary>
        /// Retrieve a list of toplevel DictionaryItems
        /// </summary>
        public static DictionaryItem[] getTopMostItems
        {
            get
            {
                EnsureCache();

                return DictionaryItems.Values
                    .Where(x => x.ParentId == TopLevelParent).OrderBy(item => item.key)
                    .ToArray();
            }
        }

        /// <summary>
        /// A DictionaryItem is basically a key/value pair (key/language key/value) which holds the data
        /// associated to a key in various language translations
        /// </summary>
        public class DictionaryItem
        {

            private string _key;

            internal Guid UniqueId { get; private set; }
            internal Guid ParentId { get; private set; }

            /// <summary>
            /// Used internally to construct a new item object and store in cache
            /// </summary>
            /// <param name="id"></param>
            /// <param name="key"></param>
            /// <param name="uniqueKey"></param>
            /// <param name="parentId"></param>
            internal DictionaryItem(int id, string key, Guid uniqueKey, Guid parentId)
            {
                this.id = id;
                this._key = key;
                this.UniqueId = uniqueKey;
                this.ParentId = parentId;
            }

            public DictionaryItem(string key)
            {
                EnsureCache();

                var item = DictionaryItems.Values.SingleOrDefault(x => x.key == key);

                if (item == null)
                {
                    throw new ArgumentException("No key " + key + " exists in dictionary");
                }

                this.id = item.id;
                this._key = item.key;
                this.ParentId = item.ParentId;
                this.UniqueId = item.UniqueId;
            }

            public DictionaryItem(Guid id)
            {
                EnsureCache();

                var item = DictionaryItems.Values.SingleOrDefault(x => x.UniqueId == id);

                if (item == null)
                {
                    throw new ArgumentException("No unique id " + id.ToString() + " exists in dictionary");
                }

                this.id = item.id;
                this._key = item.key;
                this.ParentId = item.ParentId;
                this.UniqueId = item.UniqueId;
            }

            public DictionaryItem(int id)
            {
                EnsureCache();

                var item = DictionaryItems.Values.SingleOrDefault(x => x.id == id);

                if (item == null)
                {
                    throw new ArgumentException("No id " + id + " exists in dictionary");
                }

                this.id = item.id;
                this._key = item.key;
                this.ParentId = item.ParentId;
                this.UniqueId = item.UniqueId;
            }

            private DictionaryItem _parent;

            /// <summary>
            /// Returns if the dictionaryItem is the root item.            
            /// </summary>
            public bool IsTopMostItem()
            {
                EnsureCache();
                return DictionaryItems.Values
                    .Where(x => x.id == id)
                    .Select(x => x.ParentId)
                    .SingleOrDefault() == TopLevelParent;
            }

            /// <summary>
            /// Returns the parent.
            /// </summary>
            public DictionaryItem Parent
            {
                get
                {
                    EnsureCache();
                    if (_parent == null)
                    {
                        var p = DictionaryItems.Values.SingleOrDefault(x => x.UniqueId == this.ParentId);

                        if (p == null)
                        {
                            throw new ArgumentException("Top most dictionary items doesn't have a parent");
                        }
                        else
                        {
                            _parent = p;
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
                get;
                private set;
            }

            public DictionaryItem[] Children
            {
                get
                {
                    EnsureCache();
                    return DictionaryItems.Values
                        .Where(x => x.ParentId == this.UniqueId).OrderBy(item => item.key)
                        .ToArray();
                }
            }

            public static bool hasKey(string key)
            {
                EnsureCache();
                return DictionaryItems.ContainsKey(key);
            }

            public bool hasChildren
            {
                get
                {
                    EnsureCache();
                    return DictionaryItems.Values.Any(x => x.ParentId == UniqueId);
                }
            }

            /// <summary>
            /// Returns or sets the key.
            /// </summary>
            public string key
            {
                get { return _key; }
                set
                {
                    if (!hasKey(value))
                    {
                        lock (Locker)
                        {
                            SqlHelper.ExecuteNonQuery("Update cmsDictionary set [key] = @key WHERE pk = @Id", SqlHelper.CreateParameter("@key", value),
                                SqlHelper.CreateParameter("@Id", id));
                            
                            using (IRecordsReader dr =
                                SqlHelper.ExecuteReader("Select pk, id, [key], parent from cmsDictionary where id=@id",
                            SqlHelper.CreateParameter("@id", this.UniqueId)))
                            {
                                if (dr.Read())
                                {
                                    //create new dictionaryitem object and put in cache
                                    var item = new DictionaryItem(dr.GetInt("pk"),
                                        dr.GetString("key"),
                                        dr.GetGuid("id"),
                                        dr.GetGuid("parent"));
                                }
                                else
                                {
                                    throw new DataException("Could not load updated created dictionary item with id " + id);
                                }
                            }

                            //finally update this objects value
                            this._key = value;
                        }
                    }
                    else
                        throw new ArgumentException("New value of key already exists (is key)");
                }
            }

            public string Value(int languageId)
            {
                if (languageId == 0)
                    return Value();

                if (Item.hasText(UniqueId, languageId))
                    return Item.Text(UniqueId, languageId);

                return "";
            }

            public void setValue(int languageId, string value)
            {
                if (Item.hasText(UniqueId, languageId))
                    Item.setText(languageId, UniqueId, value);
                else
                    Item.addText(languageId, UniqueId, value);

                // Calling Save method triggers the Saving event
                Save();
            }

            public string Value()
            {
                if (Item.hasText(UniqueId, 1))
                {
                    return Item.Text(UniqueId, 1);
                }

                return string.Empty;
            }

            /// <summary>
            /// This sets the value for the placeholder language (id = 0), not for a language with an ID
            /// </summary>
            /// <param name="value"></param>
            public void setValue(string value)
            {
                if (Item.hasText(UniqueId, 0))
                    Item.setText(0, UniqueId, value);
                else
                    Item.addText(0, UniqueId, value);

                // Calling Save method triggers the Saving event
                Save();
            }

            public static int addKey(string key, string defaultValue, string parentKey)
            {
                EnsureCache();

                if (hasKey(parentKey))
                {
                    int retval = createKey(key, new DictionaryItem(parentKey).UniqueId, defaultValue);
                    return retval;
                }
                else
                    throw new ArgumentException("Parentkey doesnt exist");
            }

            public static int addKey(string key, string defaultValue)
            {
                EnsureCache();
                int retval = createKey(key, TopLevelParent, defaultValue);
                return retval;
            }

            public void delete()
            {
                OnDeleting(EventArgs.Empty);

                // delete recursive
                foreach (DictionaryItem dd in Children)
                    dd.delete();

                // remove all language values from key
                Item.removeText(UniqueId);

                // remove key from database
                SqlHelper.ExecuteNonQuery("delete from cmsDictionary where [key] = @key", SqlHelper.CreateParameter("@key", key));

                OnDeleted(EventArgs.Empty);
            }

            /// <summary>
            /// ensures events fire after setting proeprties
            /// </summary>
            public void Save()
            {
                OnSaving(EventArgs.Empty);
            }


            public System.Xml.XmlNode ToXml(XmlDocument xd)
            {

                XmlNode dictionaryItem = xd.CreateElement("DictionaryItem");
                dictionaryItem.Attributes.Append(xmlHelper.addAttribute(xd, "Key", this.key));
                foreach (Language lang in Language.GetAllAsList())
                {
                    XmlNode itemValue = xmlHelper.addCDataNode(xd, "Value", this.Value(lang.id));
                    itemValue.Attributes.Append(xmlHelper.addAttribute(xd, "LanguageId", lang.id.ToString()));
                    itemValue.Attributes.Append(xmlHelper.addAttribute(xd, "LanguageCultureAlias", lang.CultureAlias));
                    dictionaryItem.AppendChild(itemValue);
                }

                if (this.hasChildren)
                {
                    foreach (DictionaryItem di in this.Children)
                    {
                        dictionaryItem.AppendChild(di.ToXml(xd));
                    }
                }


                return dictionaryItem;
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

            [MethodImpl(MethodImplOptions.Synchronized)]
            private static int createKey(string key, Guid parentId, string defaultValue)
            {
                if (!hasKey(key))
                {
                    Guid newId = Guid.NewGuid();
                    SqlHelper.ExecuteNonQuery("Insert into cmsDictionary (id,parent,[key]) values (@id, @parentId, @dictionaryKey)",
                                              SqlHelper.CreateParameter("@id", newId),
                                              SqlHelper.CreateParameter("@parentId", parentId),
                                              SqlHelper.CreateParameter("@dictionaryKey", key));

                    using (IRecordsReader dr =
                        SqlHelper.ExecuteReader("Select pk, id, [key], parent from cmsDictionary where id=@id",
                            SqlHelper.CreateParameter("@id", newId)))
                    {
                        if (dr.Read())
                        {
                            //create new dictionaryitem object and put in cache
                            var item = new DictionaryItem(dr.GetInt("pk"),
                                dr.GetString("key"),
                                dr.GetGuid("id"),
                                dr.GetGuid("parent"));
                            
                            item.setValue(defaultValue);

                            item.OnNew(EventArgs.Empty);

                            return item.id;
                        }
                        else
                        {
                            throw new DataException("Could not load newly created dictionary item with id " + newId.ToString());
                        }
                    }

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