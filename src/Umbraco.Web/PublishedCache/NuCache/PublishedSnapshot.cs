﻿using System;
using Umbraco.Core;
using Umbraco.Core.Cache;

namespace Umbraco.Web.PublishedCache.NuCache
{
    // implements published snapshot
    internal class PublishedSnapshot : IPublishedSnapshot, IDisposable
    {
        private readonly PublishedSnapshotService _service;
        private bool _defaultPreview;
        private PublishedSnapshotElements _elements;

        #region Constructors

        public PublishedSnapshot(PublishedSnapshotService service, bool defaultPreview)
        {
            _service = service;
            _defaultPreview = defaultPreview;
        }

        public class PublishedSnapshotElements : IDisposable
        {
            public ContentCache ContentCache;
            public MediaCache MediaCache;
            public MemberCache MemberCache;
            public DomainCache DomainCache;
            public IAppCache SnapshotCache;
            public IAppCache ElementsCache;

            public void Dispose()
            {
                ContentCache.Dispose();
                MediaCache.Dispose();
            }
        }

        private PublishedSnapshotElements Elements => _elements ?? (_elements = _service.GetElements(_defaultPreview));

        public void Resync()
        {
            // no lock - published snapshots are single-thread
            _elements?.Dispose();
            _elements = null;
        }

        #endregion

        #region Caches

        public IAppCache SnapshotCache => Elements.SnapshotCache;

        public IAppCache ElementsCache => Elements.ElementsCache;

        #endregion

        #region IPublishedSnapshot

        public IPublishedContentCache Content => Elements.ContentCache;

        public IPublishedMediaCache Media => Elements.MediaCache;

        public IPublishedMemberCache Members => Elements.MemberCache;

        public IDomainCache Domains => Elements.DomainCache;

        public IDisposable ForcedPreview(bool preview, Action<bool> callback = null)
        {
            return new ForcedPreviewObject(this, preview, callback);
        }

        private class ForcedPreviewObject : DisposableObjectSlim
        {
            private readonly PublishedSnapshot _publishedSnapshot;
            private readonly bool _origPreview;
            private readonly Action<bool> _callback;

            public ForcedPreviewObject(PublishedSnapshot publishedShapshot, bool preview, Action<bool> callback)
            {
                _publishedSnapshot = publishedShapshot;
                _callback = callback;

                // save and force
                _origPreview = publishedShapshot._defaultPreview;
                publishedShapshot._defaultPreview = preview;
            }

            protected override void DisposeResources()
            {
                // restore
                _publishedSnapshot._defaultPreview = _origPreview;
                _callback?.Invoke(_origPreview);
            }
        }

        #endregion

        #region IDisposable

        private bool _disposed;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _elements?.Dispose();
        }

        #endregion
    }
}
