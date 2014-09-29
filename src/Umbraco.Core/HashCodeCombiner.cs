using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Umbraco.Core
{
	/// <summary>
	/// Used to create a hash code from multiple objects.
	/// </summary>
	/// <remarks>
	/// .Net has a class the same as this: System.Web.Util.HashCodeCombiner and of course it works for all sorts of things
	/// which we've not included here as we just need a quick easy class for this in order to create a unique
	/// hash of directories/files to see if they have changed.
	/// </remarks>
	internal class HashCodeCombiner
	{
		private long _combinedHash = 5381L;

		internal void AddInt(int i)
		{
			_combinedHash = ((_combinedHash << 5) + _combinedHash) ^ i;
		}

		internal void AddObject(object o)
		{
			AddInt(o.GetHashCode());
		}

		internal void AddDateTime(DateTime d)
		{
			AddInt(d.GetHashCode());
		}

        internal void AddString(string s)
        {
            if (s != null)
                AddInt((StringComparer.InvariantCulture).GetHashCode(s));
        }

		internal void AddCaseInsensitiveString(string s)
		{
			if (s != null)
				AddInt((StringComparer.InvariantCultureIgnoreCase).GetHashCode(s));
		}

		internal void AddFileSystemItem(FileSystemInfo f)
		{
			//if it doesn't exist, don't proceed.
			if (!f.Exists)
				return;

			AddCaseInsensitiveString(f.FullName);
			AddDateTime(f.CreationTimeUtc);
			AddDateTime(f.LastWriteTimeUtc);
			
			//check if it is a file or folder 
			var fileInfo = f as FileInfo;
			if (fileInfo != null)
			{
				AddInt(fileInfo.Length.GetHashCode());
			}
			
			var dirInfo = f as DirectoryInfo;
			if (dirInfo != null)
			{
				foreach (var d in dirInfo.GetFiles())
				{
					AddFile(d);
				}
				foreach (var s in dirInfo.GetDirectories())
				{
					AddFolder(s);
				}
			}
		}

		internal void AddFile(FileInfo f)
		{
			AddFileSystemItem(f);			
		}

		internal void AddFolder(DirectoryInfo d)
		{
			AddFileSystemItem(d);			
		}

		/// <summary>
		/// Returns the hex code of the combined hash code
		/// </summary>
		/// <returns></returns>
		internal string GetCombinedHashCode()
		{
			return _combinedHash.ToString("x", CultureInfo.InvariantCulture);
		}

	}
}
