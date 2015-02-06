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
	public abstract class FileSystemWrapper : IFileSystem
	{
		private readonly IFileSystem _wrapped;

		protected FileSystemWrapper(IFileSystem wrapped)
		{
			_wrapped = wrapped;
		}

		public IEnumerable<string> GetDirectories(string path)
		{
			return _wrapped.GetDirectories(path);
		}

		public void DeleteDirectory(string path)
		{
			_wrapped.DeleteDirectory(path);
		}

		public void DeleteDirectory(string path, bool recursive)
		{
			_wrapped.DeleteDirectory(path, recursive);
		}

		public bool DirectoryExists(string path)
		{
			return _wrapped.DirectoryExists(path);
		}

		public void AddFile(string path, Stream stream)
		{
			_wrapped.AddFile(path, stream);
		}

		public void AddFile(string path, Stream stream, bool overrideIfExists)
		{
			_wrapped.AddFile(path, stream, overrideIfExists);
		}

		public IEnumerable<string> GetFiles(string path)
		{
			return _wrapped.GetFiles(path);
		}

		public IEnumerable<string> GetFiles(string path, string filter)
		{
			return _wrapped.GetFiles(path, filter);
		}

		public Stream OpenFile(string path)
		{
			return _wrapped.OpenFile(path);
		}

		public void DeleteFile(string path)
		{
			_wrapped.DeleteFile(path);
		}

		public bool FileExists(string path)
		{
			return _wrapped.FileExists(path);
		}

		public string GetRelativePath(string fullPathOrUrl)
		{
			return _wrapped.GetRelativePath(fullPathOrUrl);
		}

		public string GetFullPath(string path)
		{
			return _wrapped.GetFullPath(path);
		}

		public string GetUrl(string path)
		{
			return _wrapped.GetUrl(path);
		}

		public DateTimeOffset GetLastModified(string path)
		{
			return _wrapped.GetLastModified(path);
		}

		public DateTimeOffset GetCreated(string path)
		{
			return _wrapped.GetCreated(path);
		}
	}
}