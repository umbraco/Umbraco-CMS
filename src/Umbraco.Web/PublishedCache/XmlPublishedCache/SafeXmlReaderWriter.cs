using System;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Scoping;

namespace Umbraco.Web.PublishedCache.XmlPublishedCache
{
    // fixme should be a ScopeContextualBase
    internal class SafeXmlReaderWriter : IDisposable
    {
        private readonly bool _scoped;
        private readonly Action<XmlDocument> _refresh;
        private readonly Action<XmlDocument, bool> _apply;
        private IDisposable _releaser;
        private bool _applyChanges;
        private XmlDocument _xml, _origXml;
        private bool _using;
        private bool _registerXmlChange;

        // the default enlist priority is 100
        // enlist with a lower priority to ensure that anything "default" has a clean xml
        private const int EnlistPriority = 60;
        private const string EnlistKey = "safeXmlReaderWriter";

        private SafeXmlReaderWriter(IDisposable releaser, XmlDocument xml, Action<XmlDocument> refresh, Action<XmlDocument, bool> apply, bool isWriter, bool scoped)
        {
            _releaser = releaser;
            _refresh = refresh;
            _apply = apply;
            _scoped = scoped;

            IsWriter = isWriter;

            _xml = IsWriter ? Clone(xml) : xml;
        }

        public static SafeXmlReaderWriter Get(IScopeProvider scopeProvider)
        {
            return scopeProvider?.Context?.GetEnlisted<SafeXmlReaderWriter>(EnlistKey);
        }

        public static SafeXmlReaderWriter Get(IScopeProvider scopeProvider, AsyncLock xmlLock, XmlDocument xml, Action<XmlDocument> refresh, Action<XmlDocument, bool> apply, bool writer)
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
            var rw = scopeContext.Enlist(EnlistKey,
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
                throw new InvalidOperationException("panic: used.");
            rw._using = true;

            return rw;
        }

        public bool IsWriter { get; private set; }

        public void UpgradeToWriter(bool auto)
        {
            if (IsWriter)
                throw new InvalidOperationException("Already a writer.");
            IsWriter = true;

            _xml = Clone(_xml);
        }

        // for tests
        internal static Action Cloning { get; set; }

        private XmlDocument Clone(XmlDocument xml)
        {
            Cloning?.Invoke();
            if (_origXml != null)
                throw new Exception("panic.");
            _origXml = xml;
            return (XmlDocument) xml?.CloneNode(true);
        }

        public XmlDocument Xml
        {
            get => _xml;
            set
            {
                if (IsWriter == false)
                    throw new InvalidOperationException("Not a writer.");
                _xml = value;
            }
        }

        // registerXmlChange indicates whether to do what should be done when Xml changes,
        // that is, to request that the file be written to disk - something we don't want
        // to do if we're committing Xml precisely after we've read from disk!
        public void AcceptChanges(bool registerXmlChange = true)
        {
            if (IsWriter == false)
                throw new InvalidOperationException("Not a writer.");

            _applyChanges = true;
            _registerXmlChange |= registerXmlChange;
        }

        private void DisposeForReal(bool completed)
        {
            if (IsWriter)
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
