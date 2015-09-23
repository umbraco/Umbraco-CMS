using Lucene.Net.Store;

namespace UmbracoExamine.LocalStorage
{
    /// <summary>
    /// Lock that wraps multiple locks
    /// </summary>
    public class MultiIndexLock : Lock
    {
        private readonly Lock _dirMaster;
        private readonly Lock _dirChild;

        public MultiIndexLock(Lock dirMaster, Lock dirChild)
        {
            _dirMaster = dirMaster;
            _dirChild = dirChild;
        }

        /// <summary>
        /// Attempts to obtain exclusive access and immediately return
        ///             upon success or failure.
        /// </summary>
        /// <returns>
        /// true iff exclusive access is obtained
        /// </returns>
        public override bool Obtain()
        {
            return _dirMaster.Obtain() && _dirChild.Obtain();
        }

        public override bool Obtain(long lockWaitTimeout)
        {
            return _dirMaster.Obtain(lockWaitTimeout) && _dirChild.Obtain(lockWaitTimeout);
        }

        /// <summary>
        /// Releases exclusive access. 
        /// </summary>
        public override void Release()
        {
            _dirMaster.Release();
            _dirChild.Release();
        }

        /// <summary>
        /// Returns true if the resource is currently locked.  Note that one must
        ///             still call <see cref="M:Lucene.Net.Store.Lock.Obtain"/> before using the resource. 
        /// </summary>
        public override bool IsLocked()
        {
            return _dirMaster.IsLocked() || _dirChild.IsLocked();
        }
    }
}