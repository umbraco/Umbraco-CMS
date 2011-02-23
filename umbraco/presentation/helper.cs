using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Web;

using umbraco.BusinessLogic;
using System.Xml;
using System.Web.UI;

namespace umbraco
{
    /// <summary>
    /// Summary description for helper.
    /// </summary>
    public class helper
    {
        public static bool IsNumeric(string Number)
        {
            int result;
            return int.TryParse(Number, out result);
        }

        public static User GetCurrentUmbracoUser()
        {
            return umbraco.BasePages.UmbracoEnsuredPage.CurrentUser;
        }

        [Obsolete("Use umbraco.Presentation.UmbracoContext.Current.Request[key]", false)]
        public static string Request(string text)
        {
            string temp = string.Empty;
            if (HttpContext.Current.Request[text.ToLower()] != null)
                if (HttpContext.Current.Request[text] != string.Empty)
                    temp = HttpContext.Current.Request[text];
            return temp;
        }

        public static Hashtable ReturnAttributes(String tag)
        {
            Hashtable ht = new Hashtable();
            MatchCollection m =
                Regex.Matches(tag, "(?<attributeName>\\S*)=\"(?<attributeValue>[^\"]*)\"",
                              RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            // fix for issue 14862: return lowercase attributes for case insensitive matching
            foreach (Match attributeSet in m)
                ht.Add(attributeSet.Groups["attributeName"].Value.ToString().ToLower(), attributeSet.Groups["attributeValue"].Value.ToString());

            return ht;
        }

        public static String FindAttribute(IDictionary attributes, String key)
        {
            return FindAttribute(null, attributes, key);
        }

        public static String FindAttribute(IDictionary pageElements, IDictionary attributes, String key)
        {
            // fix for issue 14862: lowercase for case insensitive matching
            key = key.ToLower();

            string attributeValue = string.Empty;
            if (attributes[key] != null)
                attributeValue = attributes[key].ToString();

            attributeValue = parseAttribute(pageElements, attributeValue);
            return attributeValue;
        }

        public static string parseAttribute(IDictionary pageElements, string attributeValue)
        {
            // Check for potential querystring/cookie variables
            if (attributeValue.Length > 3 && attributeValue.Substring(0, 1) == "[")
            {
                string[] attributeValueSplit = (attributeValue).Split(',');
                foreach (string attributeValueItem in attributeValueSplit)
                {
                    attributeValue = attributeValueItem;

                    // Check for special variables (always in square-brackets like [name])
                    if (attributeValueItem.Substring(0, 1) == "[" &&
                        attributeValueItem.Substring(attributeValueItem.Length - 1, 1) == "]")
                    {
                        // find key name
                        string keyName = attributeValueItem.Substring(2, attributeValueItem.Length - 3);
                        string keyType = attributeValueItem.Substring(1, 1);

                        switch (keyType)
                        {
                            case "@":
                                attributeValue = HttpContext.Current.Request[keyName];
                                break;
                            case "%":
                                attributeValue = StateHelper.GetSessionValue<string>(keyName);
                                break;
                            case "#":
                                if (pageElements[keyName] != null)
                                    attributeValue = pageElements[keyName].ToString();
                                else
                                    attributeValue = "";
                                break;
                            case "$":
                                if (pageElements[keyName] != null && pageElements[keyName].ToString() != string.Empty)
                                {
                                    attributeValue = pageElements[keyName].ToString();
                                }
                                else
                                {
                                    XmlDocument umbracoXML = presentation.UmbracoContext.Current.GetXml();

                                    String[] splitpath = (String[])pageElements["splitpath"];
                                    for (int i = 0; i < splitpath.Length - 1; i++)
                                    {
                                        XmlNode element = umbracoXML.GetElementById(splitpath[splitpath.Length - i - 1].ToString());
                                        if (element == null)
                                            continue;
                                        string xpath = UmbracoSettings.UseLegacyXmlSchema ? "./data [@alias = '{0}']" : "{0}";
                                        XmlNode currentNode = element.SelectSingleNode(string.Format(xpath,
                                            keyName));
                                        if (currentNode != null && currentNode.FirstChild != null &&
                                           !string.IsNullOrEmpty(currentNode.FirstChild.Value) &&
                                           !string.IsNullOrEmpty(currentNode.FirstChild.Value.Trim()))
                                        {
                                            HttpContext.Current.Trace.Write("parameter.recursive", "Item loaded from " + splitpath[splitpath.Length - i - 1]);
                                            attributeValue = currentNode.FirstChild.Value;
                                            break;
                                        }
                                    }
                                }
                                break;
                        }

                        if (attributeValue != null)
                        {
                            attributeValue = attributeValue.Trim();
                            if (attributeValue != string.Empty)
                                break;
                        }
                        else
                            attributeValue = string.Empty;
                    }
                }
            }

            return attributeValue;
        }

        public static string SpaceCamelCasing(string text)
        {
            string s = text;

            if (2 > s.Length)
            {
                return s;
            }


            var sb = new System.Text.StringBuilder();
            var ca = s.ToCharArray();
            ca[0] = char.ToUpper(ca[0]);

            sb.Append(ca[0]);
            for (int i = 1; i < ca.Length - 1; i++)
            {
                char c = ca[i];
                if (char.IsUpper(c) && (char.IsLower(ca[i + 1]) || char.IsLower(ca[i - 1])))
                {
                    sb.Append(' ');
                }
                sb.Append(c);
            }
            sb.Append(ca[ca.Length - 1]);
            return sb.ToString();


            /* OLD way
            string _tempString = text.Substring(0, 1).ToUpper();
            for (int i = 1; i < text.Length; i++)
            {
                if (text.Substring(i, 1) == " ")
                    break;
                if (text.Substring(i, 1).ToUpper() == text.Substring(i, 1))
                    _tempString += " ";
                _tempString += text.Substring(i, 1);
            }
            return _tempString;
             */
        }

        [Obsolete("Use umbraco.presentation.UmbracContext.Current.GetBaseUrl()")]
        public static string GetBaseUrl(HttpContext Context)
        {
            return Context.Request.Url.GetLeftPart(UriPartial.Authority);
        }
    }

    /// <summary>
    /// Class that adapts an <see cref="AttributeCollection"/> to the <see cref="IDictionary"/> interface.
    /// </summary>
    public class AttributeCollectionAdapter : IDictionary
    {
        private AttributeCollection m_Collection;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeCollectionAdapter"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        public AttributeCollectionAdapter(AttributeCollection collection)
        {
            m_Collection = collection;
        }

        #region IDictionary Members

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="T:System.Collections.IDictionary"/> object.
        /// </summary>
        /// <param name="key">The <see cref="T:System.Object"/> to use as the key of the element to add.</param>
        /// <param name="value">The <see cref="T:System.Object"/> to use as the value of the element to add.</param>
        public void Add(object key, object value)
        {
            m_Collection.Add(key.ToString(), value.ToString());
        }

        /// <summary>
        /// Removes all elements from the <see cref="T:System.Collections.IDictionary"/> object.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">
        /// The <see cref="T:System.Collections.IDictionary"/> object is read-only.
        /// </exception>
        public void Clear()
        {
            m_Collection.Clear();
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.IDictionary"/> object contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.IDictionary"/> object.</param>
        /// <returns>
        /// true if the <see cref="T:System.Collections.IDictionary"/> contains an element with the key; otherwise, false.
        /// </returns>
        public bool Contains(object key)
        {
            return m_Collection[key.ToString()] != null;
        }

        /// <summary>
        /// Returns an <see cref="T:System.Collections.IDictionaryEnumerator"/> object for the <see cref="T:System.Collections.IDictionary"/> object.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IDictionaryEnumerator"/> object for the <see cref="T:System.Collections.IDictionary"/> object.
        /// </returns>
        public IDictionaryEnumerator GetEnumerator()
        {
            return new AttributeCollectionAdapterEnumerator(this);
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.IDictionary"/> object has a fixed size.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Collections.IDictionary"/> object has a fixed size; otherwise, false.
        /// </returns>
        public bool IsFixedSize
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.IDictionary"/> object is read-only.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Collections.IDictionary"/> object is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.ICollection"/> object containing the keys of the <see cref="T:System.Collections.IDictionary"/> object.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// An <see cref="T:System.Collections.ICollection"/> object containing the keys of the <see cref="T:System.Collections.IDictionary"/> object.
        /// </returns>
        public ICollection Keys
        {
            get { return m_Collection.Keys; }
        }

        /// <summary>
        /// Removes the element with the specified key from the <see cref="T:System.Collections.IDictionary"/> object.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        public void Remove(object key)
        {
            m_Collection.Remove(key.ToString());
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.ICollection"/> object containing the values in the <see cref="T:System.Collections.IDictionary"/> object.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// An <see cref="T:System.Collections.ICollection"/> object containing the values in the <see cref="T:System.Collections.IDictionary"/> object.
        /// </returns>
        public ICollection Values
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Object"/> with the specified key.
        /// </summary>
        /// <value></value>
        public object this[object key]
        {
            get { return m_Collection[key.ToString()]; }
            set { m_Collection[key.ToString()] = value.ToString(); }
        }

        #endregion

        #region ICollection Members

        /// <summary>Not implemented.</summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.ICollection"/>.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The number of elements contained in the <see cref="T:System.Collections.ICollection"/>.
        /// </returns>
        public int Count
        {
            get { return m_Collection.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection"/> is synchronized (thread safe).
        /// </summary>
        /// <value></value>
        /// <returns>true if access to the <see cref="T:System.Collections.ICollection"/> is synchronized (thread safe); otherwise, false.
        /// </returns>
        public bool IsSynchronized
        {
            get { return false; }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.
        /// </returns>
        public object SyncRoot
        {
            get { return m_Collection; }
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (object key in m_Collection.Keys)
                yield return m_Collection[(string)key];
        }

        #endregion


        /// <summary>
        /// <see cref="IDictionaryEnumerator"/> for the <see cref="AttributeCollectionAdapter"/> class.
        /// </summary>
        private class AttributeCollectionAdapterEnumerator : IDictionaryEnumerator
        {
            private AttributeCollectionAdapter m_Adapter;
            private IEnumerator m_Enumerator;

            /// <summary>
            /// Initializes a new instance of the <see cref="AttributeCollectionAdapterEnumerator"/> class.
            /// </summary>
            /// <param name="adapter">The adapter.</param>
            public AttributeCollectionAdapterEnumerator(AttributeCollectionAdapter adapter)
            {
                m_Adapter = adapter;
                m_Enumerator = ((IEnumerable)adapter).GetEnumerator();
            }

            #region IDictionaryEnumerator Members

            /// <summary>
            /// Gets both the key and the value of the current dictionary entry.
            /// </summary>
            /// <value></value>
            /// <returns>
            /// A <see cref="T:System.Collections.DictionaryEntry"/> containing both the key and the value of the current dictionary entry.
            /// </returns>
            /// <exception cref="T:System.InvalidOperationException">
            /// The <see cref="T:System.Collections.IDictionaryEnumerator"/> is positioned before the first entry of the dictionary or after the last entry.
            /// </exception>
            public DictionaryEntry Entry
            {
                get { return new DictionaryEntry(Key, Value); }
            }

            /// <summary>
            /// Gets the key of the current dictionary entry.
            /// </summary>
            /// <value></value>
            /// <returns>
            /// The key of the current element of the enumeration.
            /// </returns>
            /// <exception cref="T:System.InvalidOperationException">
            /// The <see cref="T:System.Collections.IDictionaryEnumerator"/> is positioned before the first entry of the dictionary or after the last entry.
            /// </exception>
            public object Key
            {
                get { return m_Enumerator.Current; }
            }

            /// <summary>
            /// Gets the value of the current dictionary entry.
            /// </summary>
            /// <value></value>
            /// <returns>
            /// The value of the current element of the enumeration.
            /// </returns>
            /// <exception cref="T:System.InvalidOperationException">
            /// The <see cref="T:System.Collections.IDictionaryEnumerator"/> is positioned before the first entry of the dictionary or after the last entry.
            /// </exception>
            public object Value
            {
                get { return m_Adapter[m_Enumerator.Current]; }
            }

            #endregion

            #region IEnumerator Members

            /// <summary>
            /// Gets the current element in the collection.
            /// </summary>
            /// <value></value>
            /// <returns>
            /// The current element in the collection.
            /// </returns>
            /// <exception cref="T:System.InvalidOperationException">
            /// The enumerator is positioned before the first element of the collection or after the last element.
            /// </exception>
            public object Current
            {
                get { return m_Enumerator.Current; }
            }

            /// <summary>
            /// Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>
            /// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
            /// </returns>
            /// <exception cref="T:System.InvalidOperationException">
            /// The collection was modified after the enumerator was created.
            /// </exception>
            public bool MoveNext()
            {
                return m_Enumerator.MoveNext();
            }

            /// <summary>
            /// Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            /// <exception cref="T:System.InvalidOperationException">
            /// The collection was modified after the enumerator was created.
            /// </exception>
            public void Reset()
            {
                m_Enumerator.Reset();
            }

            #endregion
        }
    }
}