using System;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Scoping;

// ReSharper disable once CheckNamespace
namespace umbraco
{
    // provides safe access to the Xml cache
    internal class SafeXmlReaderWriter : IDisposable
    {
        private readonly bool _scoped;
        private readonly Action<XmlDocument> _refresh;
        private readonly Action<XmlDocument, bool> _apply;
        private IDisposable _releaser;
        private bool _isWriter;
        private bool _applyChanges;
        private XmlDocument _xml, _origXml;
        private bool _using;
        private bool _registerXmlChange;

        // the default enlist priority is 100
        // enlist with a lower priority to ensure that anything "default" has a clean xml
        private const int EnlistPriority = 60;

        private SafeXmlReaderWriter(IDisposable releaser, XmlDocument xml, Action<XmlDocument> refresh, Action<XmlDocument, bool> apply, bool isWriter, bool scoped)
        {
            _releaser = releaser;
            _refresh = refresh;
            _apply = apply;
            _isWriter = isWriter;
            _scoped = scoped;

            _xml = _isWriter ? Clone(xml) : xml;
        }

        public static SafeXmlReaderWriter Get(IScopeProviderInternal scopeProvider)
        {
            var scopeContext = scopeProvider.Context;
            return scopeContext == null ? null : scopeContext.GetEnlisted<SafeXmlReaderWriter>("safeXmlReaderWriter");
        }

        public static SafeXmlReaderWriter Get(IScopeProviderInternal scopeProvider, AsyncLock xmlLock, XmlDocument xml, Action<XmlDocument> refresh, Action<XmlDocument, bool> apply, bool writer)
        {
            var scopeContext = scopeProvider.Context;

            // no real scope = just create a reader/writer instance
            if (scopeContext == null)
            {
                // obtain exclusive access to xml and create reader/writer
                var releaser = xmlLock.Lock();
                return new SafeXmlReaderWriter(releaser, xml, refresh, apply, writer, false);
            }

            // get or create an enlisted reader/writer
            var rw = scopeContext.Enlist("safeXmlReaderWriter",
                () => // creator
                {
                    // obtain exclusive access to xml and create reader/writer
                    var releaser = xmlLock.Lock();
                    return new SafeXmlReaderWriter(releaser, xml, refresh, apply, writer, true);
                },
                (completed, item) => // action
                {
                    item.DisposeForReal(completed);
                }, EnlistPriority);

            // ensure it's not already in-use - should never happen, just being super safe
            if (rw._using)
                throw new InvalidOperationException();
            rw._using = true;

            return rw;
        }

        public bool IsWriter { get { return _isWriter; } }

        public void UpgradeToWriter()
        {
            if (_isWriter)
                throw new InvalidOperationException("Already a writer.");
            _isWriter = true;

            _xml = Clone(_xml);
        }

        internal static Action Cloning { get; set; }

        private XmlDocument Clone(XmlDocument xml)
        {
            if (Cloning != null) Cloning();
            if (_origXml != null)
                throw new Exception("panic.");
            _origXml = xml;
            return xml == null ? null : (XmlDocument) xml.CloneNode(true);
        }

        public XmlDocument Xml
        {
            get { return _xml; }
            set
            {
                if (_isWriter == false)
                    throw new InvalidOperationException("Not a writer.");
                _xml = value;
            }
        }

        // registerXmlChange indicates whether to do what should be done when Xml changes,
        // that is, to request that the file be written to disk - something we don't want
        // to do if we're committing Xml precisely after we've read from disk!
        public void AcceptChanges(bool registerXmlChange = true)
        {
            if (_isWriter == false)
                throw new InvalidOperationException("Not a writer.");

            _applyChanges = true;
            _registerXmlChange |= registerXmlChange;
        }

        private void DisposeForReal(bool completed)
        {
            if (_isWriter)
            {
                // apply changes, or restore the original xml for the current request
                if (_applyChanges && completed)
                    _apply(_xml, _registerXmlChange);
                else
                    _refresh(_origXml);
            }

            // release the lock
            _releaser.Dispose();
            _releaser = null;
        }

        public void Dispose()
        {
            _using = false;

            if (_scoped == false)
            {
                // really dispose
                DisposeForReal(true);
            }
            else
            {
                // don't really dispose,
                // just apply the changes for the current request
                _refresh(_xml);
            }
        }
    }
}
