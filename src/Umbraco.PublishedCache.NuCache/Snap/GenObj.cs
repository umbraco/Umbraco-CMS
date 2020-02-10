﻿using System;
using System.Threading;

namespace Umbraco.Web.PublishedCache.NuCache.Snap
{
    internal class GenObj
    {
        public GenObj(long gen)
        {
            Gen = gen;
            WeakGenRef = new WeakReference(null);
        }

        public GenRef GetGenRef()
        {
            // not thread-safe but always invoked from within a lock
            var genRef = (GenRef)WeakGenRef.Target;
            if (genRef == null)
                WeakGenRef.Target = genRef = new GenRef(this);
            return genRef;
        }

        public readonly long Gen;
        public readonly WeakReference WeakGenRef;
        public int Count;

        public void Reference()
        {
            Interlocked.Increment(ref Count);
        }

        public void Release()
        {
            Interlocked.Decrement(ref Count);
        }
    }
}
