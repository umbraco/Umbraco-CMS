using System.Collections;
using System.Xml;
using System.Xml.XPath;

// source: mvpxml.codeplex.com
namespace Umbraco.Cms.Core.Xml;

public class XmlNodeListFactory
{
    private XmlNodeListFactory()
    {
    }

    #region Public members

    /// <summary>
    ///     Creates an instance of a <see cref="XmlNodeList" /> that allows
    ///     enumerating <see cref="XmlNode" /> elements in the iterator.
    /// </summary>
    /// <param name="iterator">
    ///     The result of a previous node selection
    ///     through an <see cref="XPathNavigator" /> query.
    /// </param>
    /// <returns>An initialized list ready to be enumerated.</returns>
    /// <remarks>
    ///     The underlying XML store used to issue the query must be
    ///     an object inheriting <see cref="XmlNode" />, such as
    ///     <see cref="XmlDocument" />.
    /// </remarks>
    public static XmlNodeList CreateNodeList(XPathNodeIterator? iterator) => new XmlNodeListIterator(iterator);

    #endregion Public members

    #region XmlNodeListIterator

    private class XmlNodeListIterator : XmlNodeList
    {
        private readonly XPathNodeIterator? _iterator;
        private readonly IList<XmlNode> _nodes = new List<XmlNode>();

        public XmlNodeListIterator(XPathNodeIterator? iterator) => _iterator = iterator?.Clone();

        public override int Count
        {
            get
            {
                if (!Done)
                {
                    ReadToEnd();
                }

                return _nodes.Count;
            }
        }

        /// <summary>
        ///     Flags that the iterator has been consumed.
        /// </summary>
        private bool Done { get; set; }

        /// <summary>
        ///     Current count of nodes in the iterator (read so far).
        /// </summary>
        private int CurrentPosition => _nodes.Count;

        public override IEnumerator GetEnumerator() => new XmlNodeListEnumerator(this);

        public override XmlNode? Item(int index)
        {
            if (index >= _nodes.Count)
            {
                ReadTo(index);
            }

            // Compatible behavior with .NET
            if (index >= _nodes.Count || index < 0)
            {
                return null;
            }

            return _nodes[index];
        }

        /// <summary>
        ///     Reads the entire iterator.
        /// </summary>
        private void ReadToEnd()
        {
            while (_iterator is not null && _iterator.MoveNext())
            {
                // Check IHasXmlNode interface.
                if (_iterator.Current is not IHasXmlNode node)
                {
                    throw new ArgumentException("IHasXmlNode is missing.");
                }

                _nodes.Add(node.GetNode());
            }

            Done = true;
        }

        /// <summary>
        ///     Reads up to the specified index, or until the
        ///     iterator is consumed.
        /// </summary>
        private void ReadTo(int to)
        {
            while (_nodes.Count <= to)
            {
                if (_iterator is not null && _iterator.MoveNext())
                {
                    // Check IHasXmlNode interface.
                    if (_iterator.Current is not IHasXmlNode node)
                    {
                        throw new ArgumentException("IHasXmlNode is missing.");
                    }

                    _nodes.Add(node.GetNode());
                }
                else
                {
                    Done = true;
                    return;
                }
            }
        }

        #region XmlNodeListEnumerator

        private class XmlNodeListEnumerator : IEnumerator
        {
            private readonly XmlNodeListIterator _iterator;
            private int _position = -1;

            public XmlNodeListEnumerator(XmlNodeListIterator iterator) => _iterator = iterator;

            object? IEnumerator.Current => _iterator[_position];

            #region IEnumerator Members

            void IEnumerator.Reset() => _position = -1;

            bool IEnumerator.MoveNext()
            {
                _position++;
                _iterator.ReadTo(_position);

                // If we reached the end and our index is still
                // bigger, there are no more items.
                if (_iterator.Done && _position >= _iterator.CurrentPosition)
                {
                    return false;
                }

                return true;
            }

            #endregion
        }

        #endregion XmlNodeListEnumerator
    }

    #endregion XmlNodeListIterator
}
