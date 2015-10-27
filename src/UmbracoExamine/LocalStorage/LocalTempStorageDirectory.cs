using System;
using System.IO;
using System.Linq;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;

namespace UmbracoExamine.LocalStorage
{
    /// <summary>
    /// Used to read data from local temp storage and write to both local storage and main storage
    /// </summary>    
    public class LocalTempStorageDirectory : Directory
    {
        private readonly FSDirectory _tempStorageDir;
        private readonly FSDirectory _realDirectory;
        private LockFactory _lockFactory;

        public LocalTempStorageDirectory(
            DirectoryInfo tempStorageDir,
            FSDirectory realDirectory)
        {
            if (tempStorageDir == null) throw new ArgumentNullException("tempStorageDir");
            if (realDirectory == null) throw new ArgumentNullException("realDirectory");

            _tempStorageDir = new SimpleFSDirectory(tempStorageDir);
            _realDirectory = realDirectory;
            _lockFactory = new MultiIndexLockFactory(_realDirectory, _tempStorageDir);
            
            Enabled = true;

        }
        
        /// <summary>
        /// If initialization fails, it will be disabled and then this will just wrap the 'real directory'
        /// </summary>
        internal bool Enabled { get; set; }

        [Obsolete("this is deprecated")]
        public override string[] List()
        {
            return _realDirectory.List();
        }

        public override string[] ListAll()
        {
            //always from the real dir
            return _realDirectory.ListAll();
        }

        /// <summary>Returns true if a file with the given name exists. </summary>
        public override bool FileExists(string name)
        {
            //always from the real dir
            return _realDirectory.FileExists(name);
        }

        /// <summary>Returns the time the named file was last modified. </summary>
        public override long FileModified(string name)
        {
            //always from the real dir
            return _realDirectory.FileModified(name);
        }

        /// <summary>Set the modified time of an existing file to now. </summary>
        public override void TouchFile(string name)
        {
            //always from the real dir
            _realDirectory.TouchFile(name);

            //perform on both dirs
            if (Enabled)
            {
                _tempStorageDir.TouchFile(name);
            }
        }

        /// <summary>Removes an existing file in the directory. </summary>
        public override void DeleteFile(string name)
        {   
            _realDirectory.DeleteFile(name);

            //perform on both dirs
            if (Enabled)
            {
                _tempStorageDir.DeleteFile(name);
            }
        }
    
        [Obsolete("This is deprecated")]
        public override void RenameFile(string @from, string to)
        {
            _realDirectory.RenameFile(@from, to);

            //perform on both dirs
            if (Enabled)
            {
                _tempStorageDir.RenameFile(@from, to);
            }
        }

        /// <summary>Returns the length of a file in the directory. </summary>
        public override long FileLength(string name)
        {
            //always from the real dir
            return _realDirectory.FileLength(name);
        }

        /// <summary>
        /// Creates a new, empty file in the directory with the given name.
        /// Returns a stream writing this file. 
        /// </summary>
        public override IndexOutput CreateOutput(string name)
        {
            //write to both indexes
            if (Enabled)
            {
                return new MultiIndexOutput(
                    _tempStorageDir.CreateOutput(name),
                    _realDirectory.CreateOutput(name));    
            }

            return _realDirectory.CreateOutput(name);
        }

        /// <summary>
        /// Returns a stream reading an existing file.
        /// </summary>
        public override IndexInput OpenInput(string name)
        {
            if (Enabled)
            {
                //return the reader from the cache, not the real dir
                return _tempStorageDir.OpenInput(name);    
            }

            return _realDirectory.OpenInput(name);
        }

        /// <summary>
        /// Creates an IndexInput for the file with the given name. 
        /// </summary>
        public override IndexInput OpenInput(string name, int bufferSize)
        {
            if (Enabled)
            {
                //return the reader from the cache, not the real dir
                return _tempStorageDir.OpenInput(name, bufferSize);
            }
            return _realDirectory.OpenInput(name, bufferSize);
        }
        
        /// <summary>
        /// Ensure that any writes to this file are moved to
        ///             stable storage.  Lucene uses this to properly commit
        ///             changes to the index, to prevent a machine/OS crash
        ///             from corrupting the index. 
        /// </summary>
        public override void Sync(string name)
        {
            _realDirectory.Sync(name);
            _tempStorageDir.Sync(name);
            base.Sync(name);
        }

        public override void Close()
        {
            if (Enabled)
            {
                _tempStorageDir.Close();
            }
            _realDirectory.Close();
        }

        public override Lock MakeLock(string name)
        {
            return _lockFactory.MakeLock(name);
        }

        /// <summary>
        /// Return a string identifier that uniquely differentiates
        ///             this Directory instance from other Directory instances.
        ///             This ID should be the same if two Directory instances
        ///             (even in different JVMs and/or on different machines)
        ///             are considered "the same index".  This is how locking
        ///             "scopes" to the right index.
        /// 
        /// </summary>
        public override string GetLockID()
        {
            return string.Concat(_realDirectory.GetLockID(), _tempStorageDir.GetLockID());
        }

        public override LockFactory GetLockFactory()
        {
            return _lockFactory;
            //return _realDirectory.GetLockFactory();
        }

        public override void ClearLock(string name)
        {
            _lockFactory.ClearLock(name);

            //_realDirectory.ClearLock(name);

            //if (Enabled)
            //{
            //    _tempStorageDir.ClearLock(name);
            //}
        }

        public override void SetLockFactory(LockFactory lf)
        {
            _lockFactory = lf;
            //_realDirectory.SetLockFactory(lf);
        }

        public override void Dispose()
        {
            if (Enabled)
            {
                _tempStorageDir.Dispose();    
            }
            _realDirectory.Dispose();
        }


    }
}
