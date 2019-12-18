using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

// source: mvpxml.codeplex.com

namespace Umbraco.Core.Xml
{
    class XmlNodeListFactory
    {
        private XmlNodeListFactory() { }

        #region Public members

        /// <summary>
        /// Creates an instance of a <see cref="XmlNodeList"/> that allows
        /// enumerating <see cref="XmlNode"/> elements in the iterator.
        /// </summary>
        /// <param name="iterator">The result of a previous node selection
        /// through an <see cref="XPathNavigator"/> query.</param>
        /// <returns>An initialized list ready to be enumerated.</returns>
        /// <remarks>The underlying XML store used to issue the query must be
        /// an object inheriting <see cref="XmlNode"/>, such as
        /// <see cref="XmlDocument"/>.</remarks>
        public static XmlNodeList CreateNodeList(XPathNodeIterator iterator)
        {
            return new XmlNodeListIterator(iterator);
        }

        #endregion Public members

        #region XmlNodeListIterator

        private class XmlNodeListIterator : XmlNodeList
        {
            readonly XPathNodeIterator _iterator;
            readonly IList<XmlNode> _nodes = new List<XmlNode>();

            public XmlNodeListIterator(XPathNodeIterator iterator)
            {
                _iterator = iterator.Clone();
            }

            public override System.Collections.IEnumerator GetEnumerator()
            {
                return new XmlNodeListEnumerator(this);
            }

            public override XmlNode Item(int index)
            {

                if (index >= _nodes.Count)
                    ReadTo(index);
                // Compatible behavior with .NET
                if (index >= _nodes.Count || index < 0)
                    return null;
                return _nodes[index];
            }

            public override int Count
            {
                get
                {
                    if (!_done) ReadToEnd();
                    return _nodes.Count;
                }
            }


            /// <summary>
            /// Reads the entire iterator.
            /// </summary>
            private void ReadToEnd()
            {
                while (_iterator.MoveNext())
                {
                    var node = _iterator.Current as IHasXmlNode;
                    // Check IHasXmlNode interface.
                    if (node == null)
                        throw new ArgumentException("IHasXmlNode is missing.");
                    _nodes.Add(node.GetNode());
                }
                _done = true;
            }

            /// <summary>
            /// Reads up to the specified index, or until the
            /// iterator is consumed.
            /// </summary>
            private void ReadTo(int to)
            {
                while (_nodes.Count <= to)
                {
                    if (_iterator.MoveNext())
                    {
                        var node = _iterator.Current as IHasXmlNode;
                        // Check IHasXmlNode interface.
                        if (node == null)
                            throw new ArgumentException("IHasXmlNode is missing.");
                        _nodes.Add(node.GetNode());
                    }
                    else
                    {
                        _done = true;
                        return;
                    }
                }
            }

            /// <summary>
            /// Flags that the iterator has been consumed.
            /// </summary>
            private bool Done
            {
                get { return _done; }
            }

            bool _done;

            /// <summary>
            /// Current count of nodes in the iterator (read so far).
            /// </summary>
            private int CurrentPosition
            {
                get { return _nodes.Count; }
            }

            #region XmlNodeListEnumerator

            private class XmlNodeListEnumerator : System.Collections.IEnumerator
            {
                readonly XmlNodeListIterator _iterator;
                int _position = -1;

                public XmlNodeListEnumerator(XmlNodeListIterator iterator)
                {
                    _iterator = iterator;
                }

                #region IEnumerator Members

                void System.Collections.IEnumerator.Reset()
                {
                    _position = -1;
                }


                bool System.Collections.IEnumerator.MoveNext()
                {
                    _position++;
                    _iterator.ReadTo(_position);

                    // If we reached the end and our index is still
                    // bigger, there are no more items.
                    if (_iterator.Done && _position >= _iterator.CurrentPosition)
                        return false;

                    return true;
                }

                object System.Collections.IEnumerator.Current
                {
                    get
                    {
                        return _iterator[_position];
                    }
                }

                #endregion
            }

            #endregion XmlNodeListEnumerator
        }

        #endregion XmlNodeListIterator
    }
}
