using System;
using System.Collections;
using System.Data;
using System.Xml;

using umbraco.cms.businesslogic.language;
using umbraco.DataLayer;
using umbraco.BusinessLogic;

namespace umbraco.cms.businesslogic
{
    /// <summary>
    /// The Dictionary is used for storing and retrieving language translated textpieces in Umbraco. It uses
    /// umbraco.cms.businesslogic.language.Item class as storage and can be used from the public website of umbraco
    /// all text are cached in memory.
    /// </summary>
    public class Dictionary
    {
        private static bool cacheIsEnsured = false;
        private static Hashtable DictionaryItems = new Hashtable();
        private static string _ConnString = GlobalSettings.DbDSN;
        private static Guid topLevelParent = new Guid("41c7638d-f529-4bff-853e-59a0c2fb1bde");

        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        private static void ensureCache()
        {
            if (!cacheIsEnsured)
            {
                IRecordsReader dr =
                    SqlHelper.ExecuteReader("Select id, [key] from cmsDictionary");

                while (dr.Read())
                {
                    Guid tmp = dr.GetGuid("id");
                    string key = dr.GetString("key");
                    DictionaryItems.Add(key, tmp);
                }
                dr.Close();
                cacheIsEnsured = true;
            }
        }

        /// <summary>
        /// Retrieve a list of toplevel DictionaryItems
        /// </summary>
        public static DictionaryItem[] getTopMostItems
        {
            get
            {
                ArrayList tmp = new ArrayList();
                IRecordsReader dr =
                    SqlHelper.ExecuteReader("Select [Key] from cmsDictionary where parent = '" +
                                            topLevelParent.ToString() + "' order by [key]");
                while (dr.Read())
                {
                    tmp.Add(dr.GetString("key"));
                }
                dr.Close();
                DictionaryItem[] retval = new DictionaryItem[tmp.Count];
                for (int i = 0; i < tmp.Count; i++) retval[i] = new DictionaryItem(tmp[i].ToString());
                return retval;
            }
        }

        /// <summary>
        /// A DictionaryItem is basically a key/value pair (key/language key/value) which holds the data
        /// associated to a key in various language translations
        /// </summary>
        public class DictionaryItem
        {
            private Guid _uniqueID;
            private string _key;


            public DictionaryItem(string key)
            {
                ensureCache();
                if (hasKey(key))
                {
                    _uniqueID = (Guid)DictionaryItems[key];
                    _key = key;
                }
                else throw new ArgumentException("No key " + key + " exists in dictionary");
            }

            public DictionaryItem(Guid id)
            {
                string key =
                    SqlHelper.ExecuteScalar<string>("Select [key] from cmsDictionary where id = @id",
                                                    SqlHelper.CreateParameter("@id", id));

                ensureCache();
                if (hasKey(key))
                {
                    _uniqueID = (Guid)DictionaryItems[key];
                    _key = key;
                }
                else throw new ArgumentException("No key " + key + " exists in dictionary");
            }

            public DictionaryItem(int id)
            {
                string key =
                    SqlHelper.ExecuteScalar<string>("Select [key] from cmsDictionary where pk = " + id.ToString());

                ensureCache();
                if (hasKey(key))
                {
                    _uniqueID = (Guid)DictionaryItems[key];
                    _key = key;
                }
                else throw new ArgumentException("No key " + key + " exists in dictionary");
            }

            private DictionaryItem _parent;

            /// <summary>
            /// Returns if the dictionaryItem is the root item.
            /// Modified by Richard Soeteman on 3-4-2009. The execute scalar throws an error because a Guid instead of string is returned
            /// Solves issue http://umbraco.codeplex.com/WorkItem/View.aspx?WorkItemId=21902
            /// </summary>
            public bool IsTopMostItem()
            {
                return (SqlHelper.ExecuteScalar<Guid>("Select parent from cmsDictionary where pk = " +
                                                id) == topLevelParent);
            }

            /// <summary>
            /// Returns the parent.
            /// Modified by Richard Soeteman on 3-4-2009. The execute scalar throws an error because a Guid instead of string is returned
            /// Solves issue http://umbraco.codeplex.com/WorkItem/View.aspx?WorkItemId=21903
            /// </summary>
            public DictionaryItem Parent
            {
                get
                {
                    if (_parent == null)
                    {
                        Guid parentGuid =
                            SqlHelper.ExecuteScalar<Guid>("Select parent from cmsDictionary where pk = " +
                                                    id.ToString());
                        if (parentGuid != topLevelParent)
                            _parent =
                                new DictionaryItem(parentGuid);
                        else
                            throw new ArgumentException("Top most dictionary items doesn't have a parent");
                    }

                    return _parent;
                }
            }

            public int id
            {
                get
                {
                    return SqlHelper.ExecuteScalar<int>("Select pk from cmsDictionary where [key] = '" + key + "'");
                }
            }

            public DictionaryItem[] Children
            {
                get
                {
                    ArrayList tmp = new ArrayList();
                    IRecordsReader dr =
                        SqlHelper.ExecuteReader("Select [Key] from cmsDictionary where parent=@uniqueId order by [Key]", SqlHelper.CreateParameter("@uniqueId", _uniqueID));
                    while (dr.Read())
                    {
                        tmp.Add(dr.GetString("key"));
                    }
                    dr.Close();
                    DictionaryItem[] retval = new DictionaryItem[tmp.Count];
                    for (int i = 0; i < tmp.Count; i++) retval[i] = new DictionaryItem(tmp[i].ToString());
                    return retval;
                }
            }

            public static bool hasKey(string key)
            {
                ensureCache();
                return DictionaryItems.ContainsKey(key);
            }

            public bool hasChildren
            {
                get
                {
                    return (SqlHelper.ExecuteScalar<int>("select count([key]) as tmp from cmsDictionary where parent=@uniqueId", SqlHelper.CreateParameter("@uniqueId", _uniqueID)) > 0);
                }
            }

            /// <summary>
            /// Returns or sets the key.
            /// Modified by Richard Soeteman on 3-4-2009
            /// Solves issue http://umbraco.codeplex.com/WorkItem/View.aspx?WorkItemId=21927
            /// </summary>
            public string key
            {
                get { return _key; }
                set
                {
                    if (!hasKey(value))
                    {
                        object tmp = DictionaryItems[key];

                        DictionaryItems.Remove(key);
                        SqlHelper.ExecuteNonQuery("Update cmsDictionary set [key] = @key WHERE pk = @Id", SqlHelper.CreateParameter("@key", value), SqlHelper.CreateParameter("@Id", id));
                        _key = value;
                        DictionaryItems.Add(key, tmp);

                    }
                    else
                        throw new ArgumentException("New value of key already exists (is key)");
                }
            }

            public string Value(int languageId)
            {
                if (languageId == 0)
                    return Value();

                if (Item.hasText(_uniqueID, languageId))
                    return Item.Text(_uniqueID, languageId);

                return "";
            }

            public void setValue(int languageId, string value)
            {
                if (Item.hasText(_uniqueID, languageId))
                    Item.setText(languageId, _uniqueID, value);
                else
                    Item.addText(languageId, _uniqueID, value);
            }

            public string Value()
            {
                return Item.Text(_uniqueID, 1);
            }

            public void setValue(string value)
            {
                if (Item.hasText(_uniqueID, 0))
                    Item.setText(0, _uniqueID, value);
                else
                    Item.addText(0, _uniqueID, value);
            }

            public static int addKey(string key, string defaultValue, string parentKey)
            {
                ensureCache();
                if (hasKey(parentKey))
                {
                    int retval = createKey(key, new DictionaryItem(parentKey)._uniqueID, defaultValue);
                    return retval;
                }
                else
                    throw new ArgumentException("Parentkey doesnt exist");
            }

            public static int addKey(string key, string defaultValue)
            {
                ensureCache();
                int retval = createKey(key, topLevelParent, defaultValue);
                return retval;
            }

            public void delete()
            {
                OnDeleting(EventArgs.Empty);

                // delete recursive
                foreach (DictionaryItem dd in Children)
                    dd.delete();

                // remove all language values from key
                Item.removeText(_uniqueID);

                // Remove key from cache
                DictionaryItems.Remove(key);

                // remove key from database
                SqlHelper.ExecuteNonQuery("delete from cmsDictionary where [key] ='" + key + "'");
            }

            public void Save()
            {
                OnSaving(EventArgs.Empty);
            }


            public System.Xml.XmlNode ToXml(XmlDocument xd)
            {

                XmlNode dictionaryItem = xd.CreateElement("DictionaryItem");
                dictionaryItem.Attributes.Append(xmlHelper.addAttribute(xd, "Key", this.key));
                foreach (Language lang in Language.getAll)
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

                if (!DictionaryItem.hasKey(key))
                {
                    if (parent != null)
                        DictionaryItem.addKey(key, " ", parent.key);
                    else
                        DictionaryItem.addKey(key, " ");

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

            private static int createKey(string key, Guid parentId, string defaultValue)
            {
                if (!hasKey(key))
                {
                    Guid newId = Guid.NewGuid();
                    SqlHelper.ExecuteNonQuery("Insert into cmsDictionary (id,parent,[key]) values (@id,@parentId,'" + key + "')",
                                              SqlHelper.CreateParameter("@id", newId),
                                              SqlHelper.CreateParameter("@parentId", parentId));
                    DictionaryItems.Add(key, newId);
                    DictionaryItem di = new DictionaryItem(key);
                    di.setValue(defaultValue);

                    di.OnNew(EventArgs.Empty);

                    return di.id;
                }
                else
                {
                    throw new ArgumentException("Key being added already exists!");
                }
            }

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
        }
    }
}