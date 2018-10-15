using System;
using System.Collections.Generic;
using System.IO;

namespace Umbraco.Core.IO
{
	/// <summary>
	/// All custom file systems that are based upon another IFileSystem should inherit from FileSystemWrapper
	/// </summary>
	/// <remarks>
	/// An IFileSystem is generally used as a base file system, for example like a PhysicalFileSystem or an S3FileSystem.
	/// Then, other custom file systems are wrapped upon these files systems like MediaFileSystem, etc... All of the custom
	/// file systems must inherit from FileSystemWrapper.
	/// 
	/// This abstract class just wraps the 'real' IFileSystem object passed in to its constructor.
	/// </remarks>
	public abstract class FileSystemWrapper : IFileSystem2
	{
	    protected FileSystemWrapper(IFileSystem wrapped)
		{
			Wrapped = wrapped;
		}

        internal IFileSystem Wrapped { get; set; }

	    public IEnumerable<string> GetDirectories(string path)
		{
			return Wrapped.GetDirectories(path);
		}

		public void DeleteDirectory(string path)
		{
			Wrapped.DeleteDirectory(path);
		}

		public void DeleteDirectory(string path, bool recursive)
		{
			Wrapped.DeleteDirectory(path, recursive);
		}

		public bool DirectoryExists(string path)
		{
			return Wrapped.DirectoryExists(path);
		}

		public void AddFile(string path, Stream stream)
		{
			Wrapped.AddFile(path, stream);
		}

		public void AddFile(string path, Stream stream, bool overrideExisting)
		{
			Wrapped.AddFile(path, stream, overrideExisting);
		}

		public IEnumerable<string> GetFiles(string path)
		{
			return Wrapped.GetFiles(path);
		}

		public IEnumerable<string> GetFiles(string path, string filter)
		{
			return Wrapped.GetFiles(path, filter);
		}

		public Stream OpenFile(string path)
		{
			return Wrapped.OpenFile(path);
		}

		public void DeleteFile(string path)
		{
			Wrapped.DeleteFile(path);
		}

		public bool FileExists(string path)
		{
			return Wrapped.FileExists(path);
		}

		public string GetRelativePath(string fullPathOrUrl)
		{
			return Wrapped.GetRelativePath(fullPathOrUrl);
		}

		public string GetFullPath(string path)
		{
			return Wrapped.GetFullPath(path);
		}

		public string GetUrl(string path)
		{
			return Wrapped.GetUrl(path);
		}

		public DateTimeOffset GetLastModified(string path)
		{
			return Wrapped.GetLastModified(path);
		}

		public DateTimeOffset GetCreated(string path)
		{
			return Wrapped.GetCreated(path);
		}

        // explicitely implementing - not breaking
	    long IFileSystem2.GetSize(string path)
	    {
	        var wrapped2 = Wrapped as IFileSystem2;
	        return wrapped2 == null ? Wrapped.GetSize(path) : wrapped2.GetSize(path);
	    }

        // explicitely implementing - not breaking
        bool IFileSystem2.CanAddPhysical
	    {
	        get
	        {
                var wrapped2 = Wrapped as IFileSystem2;
	            return wrapped2 != null && wrapped2.CanAddPhysical;
	        }
        }

        // explicitely implementing - not breaking
	    void IFileSystem2.AddFile(string path, string physicalPath, bool overrideIfExists, bool copy)
	    {
            var wrapped2 = Wrapped as IFileSystem2;
            if (wrapped2 == null)
                throw new NotSupportedException();
	        wrapped2.AddFile(path, physicalPath, overrideIfExists, copy);
	    }
    }
}