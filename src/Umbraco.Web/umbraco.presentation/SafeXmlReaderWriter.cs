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
        private readonly Action<XmlDocument, bool> _apply;
        private IDisposable _releaser;
        private bool _isWriter;
        private bool _applyChanges;
        private XmlDocument _xml;
        private bool _using;
        private bool _registerXmlChange;

        private SafeXmlReaderWriter(IDisposable releaser, XmlDocument xml, Action<XmlDocument, bool> apply, bool isWriter, bool scoped)
        {
            _releaser = releaser;
            _apply = apply;
            _isWriter = isWriter;
            _scoped = scoped;

            // cloning for writer is not an option anymore (see XmlIsImmutable)
            _xml = _isWriter ? Clone(xml) : xml;
        }

        public static SafeXmlReaderWriter Get(IScopeProviderInternal scopeProvider, AsyncLock xmlLock, XmlDocument xml, Action<XmlDocument, bool> apply, bool writer)
        {
            var scopeContext = scopeProvider.Context;

            // no real scope = just create a reader/writer instance
            if (scopeContext == null)
            {
                // obtain exclusive access to xml and create reader/writer
                var releaser = xmlLock.Lock();
                return new SafeXmlReaderWriter(releaser, xml, apply, writer, false);
            }

            // get or create an enlisted reader/writer
            var rw = scopeContext.Enlist("safeXmlReaderWriter",
                () => // creator
                {
                    // obtain exclusive access to xml and create reader/writer
                    var releaser = xmlLock.Lock();
                    return new SafeXmlReaderWriter(releaser, xml, apply, writer, true);
                },
                (completed, item) => // action
                {
                    item.DisposeForReal(completed);
                });

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

            // cloning for writer is not an option anymore (see XmlIsImmutable)
            //fixme: But XmlIsImmutable is not actually used!
            _xml = Clone(_xml); 
        }

        internal static Action Cloning { get; set; }

        private static XmlDocument Clone(XmlDocument xmlDoc)
        {
            if (Cloning != null) Cloning();
            return xmlDoc == null ? null : (XmlDocument) xmlDoc.CloneNode(true);
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

            // fixme - what about context cache?
            // just 'clearing' here is not enough because it would be re-assigned to the
            // un-modified _xmlContent, so we'd need to *change* it somehow to point to
            // our temp _xml (and then maybe restore if the scope does not complete).
            // not doing it means that the 'current' xml cache does *not* update during the
            // scope but only once the scope has completed... might be an issue with
            // GetUrl... would that impact Deploy? don't think so - so for the time being
            // we do nothing *but* we need to deal with it at some point!
        }

        private void DisposeForReal(bool completed)
        {
            // apply changes!
            if (_isWriter && _applyChanges && completed)
                _apply(_xml, _registerXmlChange);

            // release the lock
            _releaser.Dispose();
            _releaser = null;
        }

        public void Dispose()
        {
            _using = false;

            if (_scoped == false)
                DisposeForReal(true);
        }
    }
}