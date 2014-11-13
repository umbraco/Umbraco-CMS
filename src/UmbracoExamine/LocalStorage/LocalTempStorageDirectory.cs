using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Store;

namespace UmbracoExamine.LocalStorage
{
    public class LocalTempStorageDirectory : SimpleFSDirectory
    {
        private readonly Lucene.Net.Store.Directory _realDirectory;
        
        public LocalTempStorageDirectory(
            DirectoryInfo tempStorageDir,
            Lucene.Net.Store.Directory realDirectory)
            : base(tempStorageDir)
        {
            _realDirectory = realDirectory;
            Enabled = true;
        }

        /// <summary>
        /// If initialization fails, it will be disabled and then this will just wrap the 'real directory'
        /// </summary>
        internal bool Enabled { get; set; }

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
        }

        /// <summary>Removes an existing file in the directory. </summary>
        public override void DeleteFile(string name)
        {
            //perform on both dirs
            if (Enabled)
            {
                base.DeleteFile(name);    
            }
            
            _realDirectory.DeleteFile(name);
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
                    base.CreateOutput(name),
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
                return base.OpenInput(name);    
            }

            return _realDirectory.OpenInput(name);
        }

        public override void Dispose()
        {
            if (Enabled)
            {
                base.Dispose();    
            }
            _realDirectory.Dispose();
        }


    }
}
