using System;
using Lucene.Net.Store;

namespace UmbracoExamine.LocalStorage
{
    /// <summary>
    /// Lock factory that wraps multiple factories
    /// </summary>
    public class MultiIndexLockFactory : LockFactory
    {
        private readonly FSDirectory _master;
        private readonly FSDirectory _child;

        public MultiIndexLockFactory(FSDirectory master, FSDirectory child)
        {
            if (master == null) throw new ArgumentNullException("master");
            if (child == null) throw new ArgumentNullException("child");
            _master = master;
            _child = child;
        }

        public override Lock MakeLock(string lockName)
        {
            return new MultiIndexLock(_master.LockFactory.MakeLock(lockName), _child.LockFactory.MakeLock(lockName));
        }
        
        public override void ClearLock(string lockName)
        {
            _master.LockFactory.ClearLock(lockName);
            _child.LockFactory.ClearLock(lockName);
        }
    }
}